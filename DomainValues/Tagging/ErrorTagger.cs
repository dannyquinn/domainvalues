using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using System.Linq;
using System.Windows;
using DomainValues.Model;
using DomainValues.Parsing;
using Microsoft.VisualStudio.Text.Adornments;

namespace DomainValues.Tagging
{
    internal sealed class ErrorTagger : SimpleTagger<ErrorTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly IWpfTextView _view;
        private readonly ErrorListProvider _errorListProvider;
        private readonly ITextDocument _textDocument;
        public ErrorTagger(ITextBuffer buffer,IWpfTextView view,ErrorListProvider errorListProvider,ITextDocument textDocument) : base(buffer)
        {
            _buffer = buffer;
            _view = view;
            _errorListProvider = errorListProvider;
            _textDocument = textDocument;

            WeakEventManager<ITextBuffer,TextContentChangedEventArgs>.AddHandler(_buffer,"Changed",TextBuffer_Changed);

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
            var lineNumber = 0;
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber);
            _errorListProvider.Tasks.Clear();
            foreach (var span in Parser.GetSpans(snapshot.GetText(), true).Where(a => a.Errors.Any()))
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

            var span = line.Snapshot.CreateTrackingSpan(new Span(line.Start + index, length), SpanTrackingMode.EdgeNegative);

            foreach (var error in errors)
            {
                ErrorTask task = CreateErrorTask(line, index, error.Message);
                _errorListProvider.Tasks.Add(task);

            }
            if (errors.Any(a => !a.OutputWindowOnly))
                CreateTagSpan(span, new ErrorTag(PredefinedErrorTypeNames.SyntaxError, errors.First(a=>!a.OutputWindowOnly).Message));
        }
        private ErrorTask CreateErrorTask(ITextSnapshotLine line, int start, string text)
        {
            ErrorTask task = new ErrorTask
            {
                Text = text,
                Line = line.LineNumber,
                Column = start,
                Category = TaskCategory.Misc,
                ErrorCategory = TaskErrorCategory.Error,
                Priority = TaskPriority.Low,
                Document = _textDocument.FilePath
            };

            task.Navigate += task_Navigate;

            return task;
        }

        private void task_Navigate(object sender, EventArgs e)
        {
            ErrorTask task = (ErrorTask) sender;
            _errorListProvider.Navigate(task, new Guid("{00000000-0000-0000-0000-000000000000}"));

            var line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(task.Line);
            var point = new SnapshotPoint(line.Snapshot, line.Start.Position + task.Column);
            _view.Caret.MoveTo(point);
        }
    }
}
