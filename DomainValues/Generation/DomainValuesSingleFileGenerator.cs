using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DomainValues.Parsing;
using Microsoft.VisualStudio.Shell;
using VSLangProj80;

namespace DomainValues.Generation
{
    [ComVisible(true)]
    [Guid(DvContent.SingleFileGeneratorGuid)]
    [CodeGeneratorRegistrationWithFileExtension(typeof(DomainValuesSingleFileGenerator),"C# Domain Values Generator",vsContextGuids.vsContextGuidVCSProject,GeneratesDesignTimeSource=true,FileExtension=DvContent.DvFileExtension)]
    [ProvideObject(typeof(DomainValuesSingleFileGenerator))]
    public class DomainValuesSingleFileGenerator : BaseGeneratorWithSite
    {
        protected override string GetDefaultExtension()
        {
            return string.Concat(DvContent.DvFileExtension, ".sql");
        }

        protected override byte[] GenerateCode(string inputFileContent)
        {
            var spans = Parser.GetSpans(inputFileContent,true);

            if (spans.Any(a => a.Errors.Any()))
            {
                return Encoding.UTF8.GetBytes("Error Generating Output");
            }

            var content = SpansToContent.Convert(spans);

            return content.GetSqlBytes();
        }
    }
}
