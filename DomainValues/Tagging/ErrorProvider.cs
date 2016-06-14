using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues.Tagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(DvContent.DvContentType)]
    [TagType(typeof(ErrorTag))]
    internal class ErrorProvider : ITaggerProvider
    {
        [Import]
        ITextDocumentFactoryService TextService = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var errors = buffer.Properties.GetProperty(typeof(ErrorListProvider)) as ErrorListProvider;
            var view = buffer.Properties.GetProperty(typeof(IWpfTextView)) as IWpfTextView;

            ITextDocument doc;

            if (TextService.TryGetTextDocument(buffer, out doc) && errors != null)
            {
                return buffer.Properties.GetOrCreateSingletonProperty(() => new ErrorTagger(buffer, view, errors, doc)) as ITagger<T>;
            }
            return null;
        }
    }
}
