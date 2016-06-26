using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DomainValues.Model;
using DomainValues.Processing;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace DomainValues.Generation
{
    [ComVisible(true)]
    [Guid(DvContent.SingleFileGeneratorGuid)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "C# Domain Values Generator", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true, FileExtension = DvContent.DvFileExtension)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator), "VB Domain Values Generator", vsContextGuids.vsContextGuidVBProject, GeneratesDesignTimeSource = true, FileExtension = DvContent.DvFileExtension)]
    [ProvideObject(typeof(DomainValuesSingleFileGenerator))]
    public class DomainValuesSingleFileGenerator : BaseGeneratorWithSite
    {
        protected override string GetDefaultExtension()
        {
            return string.Concat(DvContent.DvFileExtension, ".sql");
        }

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

                    using (FileStream fileStream = File.Create(enumFilename))
                    {
                        fileStream.Write(enumBytes, 0, enumBytes.Length);
                        fileStream.Close();
                    }
                    projectItem.ProjectItems.AddFromFile(enumFilename);

                    enumCreated = true;
                }
                sqlBytes = content.GetSqlBytes();

                if (!string.IsNullOrWhiteSpace(content.CopySql))
                {
                    Solution solution = (GetProject().DTE).Solution;

                    ProjectItem item = solution.FindProjectItem(content.CopySql);

                    if (item != null)
                    {
                        var copyFile = string.Concat(item.Properties.Item("FullPath").Value, new FileInfo(InputFilePath).Name, ".sql");

                        using (FileStream fileStream = File.Create(copyFile))
                        {
                            fileStream.Write(sqlBytes, 0, sqlBytes.Length);
                            fileStream.Close();
                        }
                        var copyItem = item.ProjectItems.AddFromFile(copyFile);

                        copyItem.Properties.Item("BuildAction").Value = "None";

                        RemoveOldFiles(projectItem, codeProvider, enumCreated, item);

                        return sqlBytes;
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
