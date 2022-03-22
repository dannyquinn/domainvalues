using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.Linq;
using Microsoft.VisualStudio.Text.Adornments;
using DomainValues.Shared.Model;
using System.Windows;
using DomainValues.Shared.Processing;

namespace DomainValues.Shared.Tagging
{
    internal sealed class ErrorTagger : SimpleTagger<ErrorTag>
    {
        private readonly ITextBuffer _buffer;
        public ErrorTagger(ITextBuffer buffer) : base(buffer)
        {
            _buffer = buffer;

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
                RemoveTagSpans(trakingTagSpan => true);
                CreateTagSpans(_buffer.CurrentSnapshot);
            }
        }
        private void CreateTagSpans(ITextSnapshot snapshot)
        {
            int lineNumber = 0;
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber);

            foreach (var span in Scanner.GetSpans(snapshot.GetText(), true).Where(a => a.Errors.Any()))
            {
                if (span.LineNumber > lineNumber)
                {
                    lineNumber = span.LineNumber;
                    line = snapshot.GetLineFromLineNumber(lineNumber);
                }
                CreateTag(line, span.Start, span.Text.Length, span.Errors);
            }
        }
        private void CreateTag(ITextSnapshotLine line, int index, int length, List<Error> errors)
        {
            ITrackingSpan span = line.Snapshot.CreateTrackingSpan(new Span(line.Start + index, length), SpanTrackingMode.EdgeNegative);

            CreateTagSpan(span, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, errors.First(a=>!a.OutputWindowOnly).Message));
        }
    }
}
