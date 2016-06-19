using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DomainValues.Parsing;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace DomainValues.Generation
{
    [ComVisible(true)]
    [Guid(DvContent.SingleFileGeneratorGuid)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator),"C# Domain Values Generator",vsContextGuids.vsContextGuidVCSProject,GeneratesDesignTimeSource=true,FileExtension=DvContent.DvFileExtension)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator),"VB Domain Values Generator",vsContextGuids.vsContextGuidVBProject,GeneratesDesignTimeSource=true,FileExtension=DvContent.DvFileExtension)]
    [ProvideObject(typeof(DomainValuesSingleFileGenerator))]
    public class DomainValuesSingleFileGenerator : BaseGeneratorWithSite
    {
        protected override string GetDefaultExtension()
        {
            return string.Concat(DvContent.DvFileExtension, ".sql");
        }
        
        protected override byte[] GenerateCode(string inputFileContent)
        {
            
            var projectItem = GetProjectItem();

            var spans = Parser.GetSpans(inputFileContent,true);

            byte[] sqlBytes;
            bool enumCreated = false;

            if (!spans.Any(a => a.Errors.Any()))
            {
                var content = SpansToContent.Convert(spans);

                var enumBytes = content.GetEnumBytes(GetCodeProvider(), FileNamespace);

                if (enumBytes != null)
                {

                    var enumFilename = $"{InputFilePath}.cs";

                    using (var fileStream = File.Create(enumFilename))
                    {
                        fileStream.Write(enumBytes, 0, enumBytes.Length);
                        fileStream.Close();
                    }
                    projectItem.ProjectItems.AddFromFile(enumFilename);

                    enumCreated = true;
                }
                sqlBytes = content.GetSqlBytes();
            }
            else
            {
                sqlBytes = Encoding.UTF8.GetBytes("Error Generating Output");
            }

            foreach (ProjectItem item in projectItem.ProjectItems)
            {
                if (item.Name.Equals($"{projectItem.Name}.sql", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                if (enumCreated && item.Name.Equals($"{projectItem.Name}.cs", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                item.Delete();
            }

            return sqlBytes;
        }

        
    }
}
