using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DomainValues.Shared
{
    internal class DvFileExtension
    {
        public const string Id = ".dv";

        [Export]
        [FileExtension(Id)]
        [ContentType(DvContent.Id)]
        internal static FileExtensionToContentTypeDefinition FileExtensionToContentTypeDefinition = null;
    }
}
