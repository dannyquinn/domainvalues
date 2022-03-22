using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues.Shared.Tagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(DvContent.Id)]
    [TagType(typeof(ErrorTag))]
    internal class ErrorProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new ErrorTagger(buffer)) as ITagger<T>;
        }
    }
}
