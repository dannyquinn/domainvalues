using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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

            byte[] sqlBytes;
            bool enumCreated = false;

            if (!spans.Any(a => a.Errors.Any()))
            {
                ContentGenerator content = SpansToContent.Convert(spans);

                byte[] enumBytes = content.GetEnumBytes(codeProvider, FileNamespace);

                if (enumBytes != null)
                {
                    string enumFilename = $"{InputFilePath}.{codeProvider.FileExtension}";

                    FileMode fileMode = File.Exists(enumFilename) ? FileMode.Truncate : FileMode.Create;

                    using (FileStream fs = new FileStream(enumFilename, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        fs.Write(enumBytes, 0, enumBytes.Length);
                        fs.Close();
                    }

                    if (fileMode == FileMode.Create)
                    {
                        projectItem.ProjectItems.AddFromFile(enumFilename);

                        var moniker = GetProject().Properties.Item("TargetFrameworkMoniker").Value.ToString();

                        if (moniker.Contains(".NETStandard"))
                        {
                            IVsSolution vsSolution = (IVsSolution)GetService(typeof(SVsSolution));

                            IVsHierarchy hierarchy;

                            vsSolution.GetProjectOfUniqueName(GetProject().UniqueName, out hierarchy);

                            IVsBuildPropertyStorage storage = hierarchy as IVsBuildPropertyStorage;

                            if (storage != null)
                            {
                                uint itemId;
                                hierarchy.ParseCanonicalName(enumFilename, out itemId);
                                storage.SetItemAttribute(itemId, "AutoGen", "True");
                                storage.SetItemAttribute(itemId, "DesignTime", "True");
                                storage.SetItemAttribute(itemId, "DependentUpon", projectItem.Name);
                            }
                        }
                    }

                    enumCreated = true;
                }

                Solution solution = (GetProject().DTE).Solution;

                var relativePath = InputFilePath.Remove(0, Path.GetDirectoryName(solution.FullName).Length);

                sqlBytes = content.GetSqlBytes(relativePath);

                if (!string.IsNullOrWhiteSpace(content.CopySql))
                {
                    ProjectItem item = solution.FindProjectItem(content.CopySql);

                    if (item != null)
                    {
                        var targetPath = item.Properties.Item("FullPath").Value.ToString();

                        // Ensure the item is a folder.
                        if (Directory.Exists(targetPath))
                        {
                            var copyFile = string.Concat(targetPath, $"{projectItem.Name}.sql");

                            FileMode fileMode = File.Exists(copyFile) ? FileMode.Truncate : FileMode.Create;

                            using (FileStream fileStream = new FileStream(copyFile, fileMode, FileAccess.Write, FileShare.ReadWrite))
                            {
                                fileStream.Write(sqlBytes, 0, sqlBytes.Length);
                                fileStream.Close();
                            }

                            if (fileMode == FileMode.Create)
                                item.ProjectItems.AddFromFile(copyFile).Properties.Item("BuildAction").Value = "None";
                        }
                        else
                        {
                            GeneratorError(1, $"{content.CopySql} is not a folder", 0, 0);
                        }
                        RemoveOldFiles(projectItem, codeProvider, enumCreated, item);

                        return sqlBytes;
                    }
                    else
                    {
                        GeneratorError(1, $"Could not find {content.CopySql} for copy operation", 0, 0);
                    }
                }
            }
            else
            {
                sqlBytes = Encoding.UTF8.GetBytes("Error Generating Output");

            }

            RemoveOldFiles(projectItem, codeProvider, enumCreated, null);

            return sqlBytes;
        }

        private void RemoveOldFiles(ProjectItem projectItem, CodeDomProvider codeProvider, bool enumCreated, ProjectItem copyLocation)
        {
            foreach (ProjectItem item in projectItem.ProjectItems)
            {
                if (item.Name.Equals($"{projectItem.Name}.sql", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (enumCreated && item.Name.Equals($"{projectItem.Name}.{codeProvider.FileExtension}", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (copyLocation != null)
                {
                    foreach (ProjectItem copyItem in copyLocation.ProjectItems)
                    {
                        if (copyItem.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))
                        {
                            copyItem.Delete();
                        }
                    }
                }

                item.Delete();
            }
        }

    }
}
