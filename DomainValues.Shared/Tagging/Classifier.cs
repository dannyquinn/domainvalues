using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Tagging;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing;
using System.Windows;

namespace DomainValues.Shared.Tagging
{
    internal sealed class Classifier : SimpleTagger<ClassificationTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly IClassificationTypeRegistryService _typeRegistry;
       
        internal Classifier(IClassificationTypeRegistryService typeRegistry, ITextBuffer buffer) : base(buffer)
        {
            _buffer = buffer;
            _typeRegistry = typeRegistry;
            WeakEventManager<ITextBuffer,TextContentChangedEventArgs>.AddHandler(buffer,"Changed",TextBuffer_Changed);

            UpdateTagSpans();
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            UpdateTagSpans();
        }

        private void UpdateTagSpans()
        {
            using (Update())
            {
                RemoveTagSpans(trackingTagSpan => true);
                CreateTagSpans(_buffer.CurrentSnapshot);
            }
        }

        private void CreateTagSpans(ITextSnapshot snapshot)
        {
            int lineNumber = 0;
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(0);

            foreach (ParsedSpan span in Scanner.GetSpans(snapshot.GetText(), false))
            {
                if (span.LineNumber > lineNumber)
                {
                    lineNumber = span.LineNumber;
                    line = snapshot.GetLineFromLineNumber(lineNumber);
                }

                IClassificationType type = _typeRegistry.GetClassificationType(TokenToClassification[span.Type]);

                CreateTag(line, span.Start, span.Text.Length, type);
            }
        }

        private void CreateTag(ITextSnapshotLine line, int index, int length, IClassificationType type)
        {
            ITrackingSpan span = line.Snapshot.CreateTrackingSpan(new Span(line.Start + index, length), SpanTrackingMode.EdgeNegative);

            CreateTagSpan(span, new ClassificationTag(type));
        }

        private static readonly Dictionary<TokenType, string> TokenToClassification = new Dictionary<TokenType, string>
        {
            {TokenType.Comment, PredefinedClassificationTypeNames.Comment},

            {TokenType.NullAs, PredefinedClassificationTypeNames.Keyword},
            {TokenType.NullAs | TokenType.Parameter, PredefinedClassificationTypeNames.Literal},

            {TokenType.SpaceAs, PredefinedClassificationTypeNames.Keyword},
            {TokenType.SpaceAs | TokenType.Parameter, PredefinedClassificationTypeNames.Literal},

            {TokenType.CopySql,PredefinedClassificationTypeNames.Keyword },
            {TokenType.CopySql | TokenType.Parameter,PredefinedClassificationTypeNames.Literal },

            {TokenType.Table, PredefinedClassificationTypeNames.Keyword},
            {TokenType.Table | TokenType.Parameter, PredefinedClassificationTypeNames.Literal},

            {TokenType.Key, PredefinedClassificationTypeNames.Keyword},
            {TokenType.Key | TokenType.Parameter, PredefinedClassificationTypeNames.String},

            {TokenType.Enum, PredefinedClassificationTypeNames.Keyword},
            {TokenType.Enum | TokenType.Parameter, PredefinedClassificationTypeNames.Literal},
            {TokenType.AccessType, PredefinedClassificationTypeNames.Keyword},
            {TokenType.BaseType, PredefinedClassificationTypeNames.Keyword},
            {TokenType.FlagsAttribute, PredefinedClassificationTypeNames.Keyword},

            {TokenType.Template,  PredefinedClassificationTypeNames.Keyword},
            {TokenType.EnumDesc, PredefinedClassificationTypeNames.String},
            {TokenType.EnumMember,PredefinedClassificationTypeNames.String},
            {TokenType.EnumInit, PredefinedClassificationTypeNames.String},

            {TokenType.Data,  PredefinedClassificationTypeNames.Keyword},

            {TokenType.HeaderRow, PredefinedClassificationTypeNames.String},

            {TokenType.ItemRow,PredefinedClassificationTypeNames.Literal},

            {TokenType.Parameter, PredefinedClassificationTypeNames.Literal},
        };
    }
}
