using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DomainValues.Tagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(DvContent.DvContentType)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ClassifierProvider : ITaggerProvider
    {
        private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;

        [ImportingConstructor]
        public ClassifierProvider(IClassificationTypeRegistryService classificationTypeRegistryService)
        {
            _classificationTypeRegistryService = classificationTypeRegistryService;
        }
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new Classifier(buffer, _classificationTypeRegistryService)) as ITagger<T>;
        }
    }
}
