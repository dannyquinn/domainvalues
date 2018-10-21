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
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;

namespace DomainValues.Command
{
    [Guid(DvContent.SingleFileGeneratorGuid)]
    [ProvideCodeGenerator(typeof(SingleFileGenerator),DvContent.SingleFileGeneratorName,"C# Domain Values Generator",true,ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid)]
    [ProvideCodeGenerator(typeof(SingleFileGenerator),DvContent.SingleFileGeneratorName,"VB Domain Values Generator",true,ProjectSystem = ProvideCodeGeneratorAttribute.VisualBasicProjectGuid)]
    [ProvideCodeGeneratorExtension(DvContent.SingleFileGeneratorName,DvContent.DvFileExtension)]
    public class SingleFileGenerator : BaseCodeGeneratorWithSite
    {
        public override string GetDefaultExtension() => $"{DvContent.DvFileExtension}.sql";
        
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
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
                    Solution solution = (GetProjectItem().ContainingProject.DTE).Solution;

                    ProjectItem item = solution.FindProjectItem(content.CopySql);


                    if (item != null)
                    {
                        item.ProjectItems.Cast<ProjectItem>().SingleOrDefault(a => a.Name == $"{projectItem.Name}.sql")?.Delete();

                        var targetPath = item.Properties.Item("FullPath").Value.ToString();

                        // Ensure the item is a folder.
                        if (Directory.Exists(targetPath))
                        {
                            var copyFile = string.Concat(targetPath, new FileInfo(InputFilePath).Name, ".sql");

                            using (FileStream fileStream = File.Create(copyFile))
                            {
                                fileStream.Write(sqlBytes, 0, sqlBytes.Length);
                                fileStream.Close();
                            }

                            item.ProjectItems.AddFromFile(copyFile).Properties.Item("BuildAction").Value = "None";
                        }
                        else
                        {
                            GeneratorErrorCallback(true,1, $"{content.CopySql} is not a folder", 0, 0);
                        }
                        RemoveOldFiles(projectItem, codeProvider, enumCreated, item);

                        return sqlBytes;
                    }
                    else
                    {
                        GeneratorErrorCallback(true,1,$"Could not find {content.CopySql} for copy operation",0,0);
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
        protected virtual CodeDomProvider GetCodeProvider()
        {
            if (_codeDomProvider != null)
                return _codeDomProvider;

            if (GetService(typeof(SVSMDCodeDomProvider)) is IVSMDCodeDomProvider provider)
            {
                _codeDomProvider = provider.CodeDomProvider as CodeDomProvider;
            }
            else
            {
                //In the case where no language specific CodeDom is available, fall back to C# 
                _codeDomProvider = CodeDomProvider.CreateProvider("CSharp");
            }
            return _codeDomProvider;
        }

        private CodeDomProvider _codeDomProvider;

        protected ProjectItem GetProjectItem()
        {
            return (ProjectItem)GetService(typeof(ProjectItem));
        }
    }
}
