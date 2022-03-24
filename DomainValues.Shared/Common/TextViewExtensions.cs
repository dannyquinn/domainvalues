using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Common
{
    internal static class TextViewExtensions
    {
        public static void CommentSelection(this ITextView textView)
        {
            var (start, end) = textView.GetSelectionLineBounds();

            using (var edit = textView.TextBuffer.CreateEdit())
            {
                while (start <= end)
                {
                    var line = edit.Snapshot.GetLineFromLineNumber(start++);

                    edit.Insert(line.Start, "#");
                }

                if (edit.HasEffectiveChanges)
                {
                    edit.Apply();
                }
            }
        }

        public static void UncommentSelection(this ITextView textView)
        {
            var (start, end) = textView.GetSelectionLineBounds();

            using (var edit = textView.TextBuffer.CreateEdit())
            {
                while (start <= end)
                {
                    var line = edit.Snapshot.GetLineFromLineNumber(start++);

                    var text = line.GetText();

                    if (text.TrimStart().StartsWith("#"))
                    {
                        var index = text.IndexOf("#");

                        var span = new Span(line.Start + index, 1);

                        edit.Replace(span, string.Empty);
                    }
                }

                if (edit.HasEffectiveChanges)
                {
                    edit.Apply();
                }
            }
        }

        public static void AlignTable(this ITextView textView)
        {
            var point = textView.Caret.Position.BufferPosition;

            if (!LineQualifies(point) || IsPipeEscaped(point))
            {
                return;
            }

            textView.Caret.MoveToPreviousCaretPosition();

            using (var edit = textView.TextBuffer.CreateEdit())
            {
                var blockStart = point.GetContainingLine().LineNumber;

                var blockEnd = blockStart;

                while (blockStart > 0)
                {
                    var lineText = textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(--blockStart).GetText().TrimStart();

                    if (lineText.StartsWith("table", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                while (blockEnd < textView.TextBuffer.CurrentSnapshot.LineCount - 1)
                {
                    var lineText = textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(++blockEnd).GetText().TrimStart();

                    if (lineText.StartsWith("table", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                AlignRows(edit, blockStart, blockEnd);

                if (edit.HasEffectiveChanges)
                {
                    edit.Apply();
                }

                textView.Caret.MoveToNextCaretPosition();

                textView.Caret.EnsureVisible();
            }
        }

        public static (int start, int end) GetSelectionLineBounds(this ITextView textView)
        {
            if (textView.Selection.IsEmpty)
            {
                var lineNo = textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

                return (lineNo, lineNo);
            }

            var start = textView.Selection.Start.Position.GetContainingLine().LineNumber;
            var end = textView.Selection.End.Position.GetContainingLine().LineNumber;

            return (start, end);
        }

        private static void AlignRows(ITextEdit edit, int start, int end)
        {
            var lines = edit.Snapshot.Lines.Where(a =>
                a.LineNumber >= start &&
                a.LineNumber <= end &&
                a.GetText().TrimStart().StartsWith("|")
            ).ToList();

            var lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

            var maxColumns = lineColumns.Max(a => a.Count);

            foreach (var line in lines)
            {
                var lineText = line.GetText();

                var firstPipe = lineText.IndexOf('|');

                var existingText = lineText.Substring(0, firstPipe);
                var wantedText = new string('\t', 2);

                if (existingText.Equals(wantedText))
                    continue;

                var span = new Span(line.Start, firstPipe);

                edit.Replace(span, wantedText);
            }

            for (var column = 0; column < maxColumns; column++)
            {
                var cols = lineColumns.Where(a => a.Count >= column + 1).ToList();
                var maxColLen = cols.Max(a => a[column].Item2.Length);

                foreach (var col in cols)
                {
                    var currentText = col[column].Item2;

                    var newText = $" {currentText}{new string(' ', maxColLen - currentText.Length)} ";

                    if (col[column].Item3 != newText)
                    {
                        edit.Replace(col[column].Item1, newText);
                    }
                }

            }
        }

        private static List<Tuple<Span, string, string>> GetColumns(SnapshotSpan span)
        {
            return Extensions.Columns.Matches(span.GetText()).Cast<Match>()
                .Select(a => Tuple.Create(new Span(span.Start + a.Index, a.Length), a.Value.Trim(), a.Value))
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
