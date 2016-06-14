using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DomainValues.Parsing;
using DomainValues.Util;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace DomainValues.Tagging
{
    internal sealed class Classifier : SimpleTagger<ClassificationTag>
    {
        private readonly ITextBuffer _textBuffer;
        private readonly IClassificationTypeRegistryService _typeService;


        internal Classifier(ITextBuffer buffer, IClassificationTypeRegistryService typeService) : base(buffer)
        {
            _textBuffer = buffer;
            _typeService = typeService;
            WeakEventManager<ITextBuffer, TextContentChangedEventArgs>.AddHandler(_textBuffer, "Changed", TextBuffer_Changed);

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
                CreateTagSpans(_textBuffer.CurrentSnapshot);
            }
        }

        private void CreateTagSpans(ITextSnapshot snapshot)
        {
            var tokenToClassification = new Dictionary<TokenType, string>
            {
                {TokenType.Table, DvContent.DvKeyword},
                {TokenType.Key, DvContent.DvKeyword},
                {TokenType.Comment, DvContent.DvComment},
                {TokenType.HeaderRow, DvContent.DvHeaderRow},
                {TokenType.Data, DvContent.DvKeyword},
                {TokenType.ItemRow, DvContent.DvText},
                {TokenType.Parameter,DvContent.DvText },
                {TokenType.Parameter|TokenType.Table, DvContent.DvText},
                {TokenType.Variable|TokenType.Key, DvContent.DvVariable}
            };

            var lineNumber = 0;
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(0);
            foreach (var span in Parser.GetSpans(snapshot.GetText(), false))
            {
                if (span.LineNumber > lineNumber)
                {
                    lineNumber = span.LineNumber;
                    line = snapshot.GetLineFromLineNumber(lineNumber);

                }
                CreateTag(line, span.Start, span.Text.Length, _typeService.GetClassificationType(tokenToClassification[span.Type]));
            }
        }

        private void CreateTag(ITextSnapshotLine line, int index, int length, IClassificationType type)
        {
            var span = line.Snapshot.CreateTrackingSpan(new Span(line.Start + index, length), SpanTrackingMode.EdgeNegative);

            CreateTagSpan(span, new ClassificationTag(type));
        }


    }
}
