#if DV_LEGACY

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DomainValues.Shared.Processing;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace DomainValues.Shared.CommandLegacy
{

    [ComVisible(true)]
    [Guid("B85060CC-947E-471B-B521-712C7193DEDA")]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "C# Domain Values Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true, FileExtension = DvFileExtension.Id)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "VB Domain Values Generator", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true, FileExtension = DvFileExtension.Id)]
    [ProvideObject(typeof(DomainValuesSingleFileGenerator))]
    public class DomainValuesSingleFileGenerator : BaseGeneratorWithSite
    {
        protected override string GetDefaultExtension() => $"{DvFileExtension.Id}.sql";

        protected override byte[] GenerateCode(string inputFileContent)
        {
            var projectItem = GetProjectItem();

            var codeProvider = GetCodeProvider();

            var spans = Scanner.GetSpans(inputFileContent, true);

            var originalName = GetLastKnownFileName(projectItem);

            if (!originalName.Equals(projectItem.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                RemoveOrphanedItem(projectItem, $"{originalName}.{codeProvider.FileExtension}");
            }

            if (spans.Any(a => a.Errors.Any()))
            {
                return Encoding.UTF8.GetBytes("Error Generating Content");
            }

            var content = SpansToContent.Convert(spans);

            var enumBytes = Encoding.UTF8.GetBytes(content.GetEnumCode(codeProvider, FileNamespace));

            if (enumBytes != null)
            {
                CreateEnumFile(codeProvider, enumBytes, projectItem);
            }
            else
            {
                foreach (ProjectItem item in projectItem.ProjectItems)
                {
                    if (item.Name.EndsWith(".cs"))
                    {
                        item.Delete();
                    }
                }
            }

            var solution = projectItem.ContainingProject.DTE.Solution;

            var relativePath = InputFilePath.Remove(0, Path.GetDirectoryName(solution.FullName)?.Length ?? 0);

            var sqlBytes = Encoding.UTF8.GetBytes(content.GetSqlCode(relativePath));

            if (string.IsNullOrWhiteSpace(content.CopySql))
                return sqlBytes;

            var copyLocation = solution.FindProjectItem(content.CopySql);

            if (copyLocation == null)
            {
                GeneratorError(1, $"Could not find {content.CopySql} for copy operation", 0, 0);
                return sqlBytes;
            }

            var targetPath = copyLocation.Properties.Item("FullPath").Value.ToString();

            if (!Directory.Exists(targetPath))
            {
                GeneratorError(1, $"{content.CopySql} is not a folder", 0, 0);
                return sqlBytes;
            }

            if (!projectItem.Name.Equals(originalName, StringComparison.InvariantCultureIgnoreCase))
            {
                RemoveOrphanedItem(copyLocation, $"{originalName}.sql");
            }

            var copyFile = $"{targetPath}{projectItem.Name}.sql";

            if (WriteFile(copyFile, sqlBytes))
            {
                copyLocation.ProjectItems.AddFromFile(copyFile).Properties.Item("BuildAction").Value = "None";
            }

            return sqlBytes;
        }

        private static bool WriteFile(string path, byte[] content)
        {
            var fileMode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;

            using (var fs = new FileStream(path, fileMode, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.Write(content, 0, content.Length);
                fs.Close();
            }

            return fileMode == FileMode.Create;
        }
        private void CreateEnumFile(CodeDomProvider codeProvider, byte[] enumBytes, ProjectItem projectItem)
        {
            var enumFileName = $"{InputFilePath}.{codeProvider.FileExtension}";

            if (!WriteFile(enumFileName, enumBytes))
            {
                return;
            }

            projectItem.ProjectItems.AddFromFile(enumFileName);

            var moniker = projectItem.ContainingProject.Properties.Item("TargetFrameworkMoniker").Value.ToString();

            if (!moniker.Contains(".NETStandard") && !moniker.Contains(".NETCoreApp"))
                return;

            var soln = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

            soln.GetProjectOfUniqueName(projectItem.ContainingProject.UniqueName, out IVsHierarchy hierarchy);

            var storage = hierarchy as IVsBuildPropertyStorage;

            if (storage == null)
            {
                return;
            }

            hierarchy.ParseCanonicalName(enumFileName, out uint itemId);
            storage.SetItemAttribute(itemId, "AutoGen", "True");
            storage.SetItemAttribute(itemId, "DependentUpon", projectItem.Name);
        }
        private static void RemoveOrphanedItem(ProjectItem parent, string name)
        {
            if (parent.ProjectItems.Count == 0)
            {
                return;
            }

            foreach (ProjectItem child in parent.ProjectItems)
            {
                if (!child.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                child.Delete();
                break;
            }
        }

        private static string GetLastKnownFileName(ProjectItem item)
        {
            var name = item.Name;

            var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));

            solution.GetProjectOfUniqueName(item.ContainingProject.UniqueName, out IVsHierarchy hierarchy);

            var storage = hierarchy as IVsBuildPropertyStorage;

            if (storage == null)
            {
                return name;
            }

            hierarchy.ParseCanonicalName(item.FileNames[0], out uint itemId);

            storage.GetItemAttribute(itemId, "LastKnownName", out string originalName);

            if (originalName == name)
            {
                return name;
            }

            storage.SetItemAttribute(itemId, "LastKnownName", name);

            return string.IsNullOrWhiteSpace(originalName) ? name : originalName;
        }
    }
}
#endif 