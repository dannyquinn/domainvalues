using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DomainValues.Shared
{
    internal class DvContent
    {
        public const string Id = "domainvalues";

        [Export]
        [Name(Id)]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;
    }
}
