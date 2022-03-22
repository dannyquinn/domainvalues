using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DomainValues.Shared.Tagging
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(DvContent.Id)]
    [TagType(typeof(ClassificationTag))]
    internal sealed class ClassifierProvider : ITaggerProvider
    {
        private readonly IClassificationTypeRegistryService _typeRegistry;

        [ImportingConstructor]
        public ClassifierProvider(IClassificationTypeRegistryService typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new Classifier(_typeRegistry, buffer)) as ITagger<T>;
        }
    }
}
