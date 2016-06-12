using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues
{
    internal class DvContent
    {
        [Export]
        [Name(DvContentType)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;

        [Export]
        [FileExtension(DvFileExtension)]
        [ContentType(DvContentType)]
        internal static FileExtensionToContentTypeDefinition FileExtensionToContentTypeDefinition = null;

        public const string DvContentType = "domainvalues";

        public const string DvFileExtension = ".dv";

        public const string DvKeyword = "domainvalues.keyword";
        public const string DvVariable = "domainvalues.variable";
        public const string DvComment = "domainvalues.comment";
        public const string DvText = "domainvalues.text";
        public const string DvHeaderRow = "domainvalues.headerrow";

        public const string SingleFileGeneratorGuid = "B85060CC-947E-471B-B521-712C7193DEDA";
    }
}
