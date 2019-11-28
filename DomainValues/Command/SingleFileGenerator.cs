using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DomainValues.Command.SingleFileBaseGenerator;
using DomainValues.Model;
using DomainValues.Processing;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj80;

namespace DomainValues.Command
{

    [ComVisible(true)]
    [Guid(DvContent.SingleFileGeneratorGuid)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "C# Domain Values Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true, FileExtension = DvContent.DvFileExtension)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "VB Domain Values Generator", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true, FileExtension = DvContent.DvFileExtension)]
    [ProvideObject(typeof(DomainValuesSingleFileGenerator))]
    public class DomainValuesSingleFileGenerator : BaseGeneratorWithSite
    {
        protected override string GetDefaultExtension() => $"{DvContent.DvFileExtension}.sql";

        protected override byte[] GenerateCode(string inputFileContent)
        {
            ProjectItem projectItem = GetProjectItem();
            CodeDomProvider codeProvider = GetCodeProvider();
            List<ParsedSpan> spans = Scanner.GetSpans(inputFileContent, true);

            string originalName = GetLastKnownFileName(projectItem);

            if (!originalName.Equals(projectItem.Name, StringComparison.InvariantCultureIgnoreCase))
                RemoveOrphanedItem(projectItem, $"{originalName}.{codeProvider.FileExtension}");

            if (spans.Any(a => a.Errors.Any()))
                return Encoding.UTF8.GetBytes("Error Generating Content");

            ContentGenerator content = SpansToContent.Convert(spans);

            byte[] enumBytes = content.GetEnumBytes(codeProvider, FileNamespace);

            if (enumBytes != null)
            {
                CreateEnumFile(codeProvider, enumBytes, projectItem);
            }
            else
            {
                foreach (ProjectItem item in projectItem.ProjectItems)
                {
                    if (item.Name.EndsWith(".cs"))
                        item.Delete();
                }
            }

            Solution solution = projectItem.ContainingProject.DTE.Solution;

            var relativePath = InputFilePath.Remove(0, Path.GetDirectoryName(solution.FullName)?.Length ?? 0);

            byte[] sqlBytes = content.GetSqlBytes(relativePath);

            if (string.IsNullOrWhiteSpace(content.CopySql))
                return sqlBytes;

            ProjectItem copyLocation = solution.FindProjectItem(content.CopySql);

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
                RemoveOrphanedItem(copyLocation, $"{originalName}.sql");

            string copyFile = $"{targetPath}{projectItem.Name}.sql";

            if (WriteFile(copyFile, sqlBytes))
                copyLocation.ProjectItems.AddFromFile(copyFile)
                    .Properties.Item("BuildAction").Value = "None";

            return sqlBytes;
        }

        private static bool WriteFile(string path, byte[] content)
        {
            FileMode fileMode = File.Exists(path) ? FileMode.Truncate : FileMode.Create;

            using (FileStream fs = new FileStream(path, fileMode, FileAccess.Write, FileShare.ReadWrite))
            {
                fs.Write(content,0,content.Length);
                fs.Close();
            }

            return fileMode == FileMode.Create;
        }
        private void CreateEnumFile(CodeDomProvider codeProvider, byte[] enumBytes, ProjectItem projectItem)
        {
            string enumFileName = $"{InputFilePath}.{codeProvider.FileExtension}";

            if (!WriteFile(enumFileName, enumBytes))
                return;

            projectItem.ProjectItems.AddFromFile(enumFileName);

            var moniker = projectItem.ContainingProject.Properties.Item("TargetFrameworkMoniker").Value.ToString();

            if (!moniker.Contains(".NETStandard") && !moniker.Contains(".NETCoreApp"))
                return;

            IVsSolution soln = (IVsSolution) Package.GetGlobalService(typeof(SVsSolution));

            soln.GetProjectOfUniqueName(projectItem.ContainingProject.UniqueName, out IVsHierarchy hierarchy);

            IVsBuildPropertyStorage storage = hierarchy as IVsBuildPropertyStorage;

            if (storage == null)
                return;

            hierarchy.ParseCanonicalName(enumFileName, out uint itemId);
            storage.SetItemAttribute(itemId, "AutoGen", "True");
            storage.SetItemAttribute(itemId, "DependentUpon", projectItem.Name);
        }
        private static void RemoveOrphanedItem(ProjectItem parent, string name)
        {
            if (parent.ProjectItems.Count == 0)
                return;

            foreach (ProjectItem child in parent.ProjectItems)
            {
                if (!child.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                child.Delete();
                break;
            }
        }

        private static string GetLastKnownFileName(ProjectItem item)
        {
            string name = item.Name;

            IVsSolution solution = (IVsSolution) Package.GetGlobalService(typeof(SVsSolution));

            solution.GetProjectOfUniqueName(item.ContainingProject.UniqueName, out IVsHierarchy hierarchy);

            IVsBuildPropertyStorage storage = hierarchy as IVsBuildPropertyStorage;

            if (storage == null)
                return name;

            hierarchy.ParseCanonicalName(item.FileNames[0], out uint itemId);

            storage.GetItemAttribute(itemId, "LastKnownName", out string originalName);

            if (originalName == name)
                return name;
            
            storage.SetItemAttribute(itemId, "LastKnownName", name);

            return string.IsNullOrWhiteSpace(originalName) ? name : originalName;
        }
    }
}
