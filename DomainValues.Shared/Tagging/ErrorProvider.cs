using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues.Shared.Tagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(DvContent.Id)]
    [TagType(typeof(ErrorTag))]
    internal class ErrorProvider : ITaggerProvider
    {
        private readonly ITextDocumentFactoryService _textDocumentFactoryService;

        [ImportingConstructor]
        public ErrorProvider(ITextDocumentFactoryService textDocumentFactoryService)
        {
            _textDocumentFactoryService = textDocumentFactoryService;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            ErrorListProvider errors = buffer.Properties.GetProperty(typeof(ErrorListProvider)) as ErrorListProvider;
            IWpfTextView view = buffer.Properties.GetProperty(typeof(IWpfTextView)) as IWpfTextView;

            ITextDocument doc;

            if (_textDocumentFactoryService.TryGetTextDocument(buffer, out doc) && errors != null)
            {
                return buffer.Properties.GetOrCreateSingletonProperty(() => new ErrorTagger(buffer, view, errors, doc)) as ITagger<T>;
            }
            return null;
        }
    }
}
