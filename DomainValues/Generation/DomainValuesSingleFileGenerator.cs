using System.Runtime.InteropServices;
using System.Text;
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
            return Encoding.UTF8.GetBytes("TODO");
        }
    }
}
