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
        private readonly IClassificationTypeRegistryService _typeRegistry;
        private readonly IClassificationFormatMapService _formatMap;
        [ImportingConstructor]
        public ClassifierProvider(IClassificationTypeRegistryService typeRegistry,IClassificationFormatMapService formatMap)
        {
            _typeRegistry = typeRegistry;
            _formatMap = formatMap;
        }
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new Classifier(_typeRegistry,_formatMap,buffer)) as ITagger<T>;
        }
    }
}
