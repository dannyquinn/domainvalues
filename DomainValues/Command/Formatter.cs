using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Util;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace DomainValues.Command
{
    internal sealed class Formatter
    {
        private readonly IWpfTextView _view;

        public Formatter(IWpfTextView view)
        {
            _view = view;
        }

        public void CommentSelection()
        {
            int start, end;

            GetSelectionLines(out start, out end);

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            {
                while (start <= end)
                {
                    var line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(start);

                    edit.Insert(line.Start, "#");

                    start++;
                }

                if (edit.HasEffectiveChanges)
                    edit.Apply();
            }

        }

        public void RemoveCommentSelection()
        {
            int start, end;

            GetSelectionLines(out start,out end);

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            {
                while (start <= end)
                {
                    var line = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(start);

                    var text = line.GetText();

                    if (text.StartsWith("#"))
                    {
                        Span span = new Span(line.Start.Position+text.IndexOf('#'),1);

                        edit.Replace(span, string.Empty);
                    }

                    start++;
                }
                if (edit.HasEffectiveChanges)
                    edit.Apply();
            }
        }

        public void AlignTable()
        {
            // Separated from align rows in preparation for the FormatDocument logic.
            SnapshotPoint point = _view.Caret.Position.BufferPosition;

            if (!LineQualifies(point) || IsPipeEscaped(point))
                return;

            _view.Caret.MoveToPreviousCaretPosition();

            using (ITextEdit edit = _view.TextBuffer.CreateEdit())
            {
                AlignRows(edit, point.Position);

                if (edit.HasEffectiveChanges)
                    edit.Apply();
            }

            _view.Caret.MoveToNextCaretPosition();

            _view.Caret.EnsureVisible();
        }
        private void AlignRows(ITextEdit edit,int position)
        {
            int blockStart = _view.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);

            int blockEnd = blockStart;

            while (blockStart > 0)
            {
                var lineText = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(--blockStart).GetText().TrimStart();

                if (lineText.StartsWith("table", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }

            while (blockEnd < _view.TextBuffer.CurrentSnapshot.LineCount - 1)
            {
                var lineText = _view.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(++blockEnd).GetText().TrimStart();

                if (lineText.StartsWith("table", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }
            }

            List<ITextSnapshotLine> lines = _view.TextBuffer.CurrentSnapshot.Lines
                .Where(a => a.LineNumber >= blockStart && a.LineNumber <= blockEnd && a.GetText().TrimStart().StartsWith("|")).ToList();

            List<List<Tuple<Span, string>>> lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

            int maxColumns = lineColumns.Max(a => a.Count);

            lines
                .Select(a => new Span(a.Start, a.GetText().IndexOf('|')))
                .ToList()
                .ForEach(a => edit.Replace(a, new string(' ', 8)));

            for (int column = 0; column < maxColumns; column++)
            {
                List<List<Tuple<Span, string>>> cols = lineColumns.Where(a => a.Count >= column + 1).ToList();
                int maxColLen = cols.Max(a => a[column].Item2.Length);

                foreach (List<Tuple<Span, string>> col in cols)
                {
                    string currentText = col[column].Item2;

                    string newText = $" {currentText}{new string(' ', maxColLen - currentText.Length)} ";

                    edit.Replace(col[column].Item1, newText);
                }

            }
        }

        private void GetSelectionLines(out int start, out int end)
        {
            if (_view.Selection.IsEmpty)
            {
                int lineNumber = _view.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

                start = end = lineNumber;
                return;
            }

            start = _view.Selection.Start.Position.GetContainingLine().LineNumber;
            end = _view.Selection.End.Position.GetContainingLine().LineNumber;
        }

        private static List<Tuple<Span, string>> GetColumns(SnapshotSpan span)
        {
            return RegExpr.Columns.Matches(span.GetText()).Cast<Match>()
                .Select(a => Tuple.Create(new Span(span.Start + a.Index, a.Length), a.Value.Trim()))
                .ToList();
        }

        private static bool LineQualifies(SnapshotPoint point)
        {
            string lineText = point.GetContainingLine().GetText();

            return lineText.TrimStart().StartsWith("|") &&
                   Regex.Matches(lineText, @"\|", RegexOptions.Compiled).Count >= 2;
        }

        private static bool IsPipeEscaped(SnapshotPoint point)
        {
            ITextSnapshotLine lineText = point.GetContainingLine();

            return lineText
                       .GetText()
                       .Substring(0, point.Position - lineText.Extent.Span.Start - 1)
                       .Reverse()
                       .TakeWhile(a => a == 92) // 92 = \
                       .Count() % 2 == 1;
        }
    }
}
