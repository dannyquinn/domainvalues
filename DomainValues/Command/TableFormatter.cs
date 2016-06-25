using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Util;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace DomainValues.Command
{
    internal static class TableFormatter
    {
        public static void Align(IWpfTextView view)
        {
            SnapshotPoint point = view.Caret.Position.BufferPosition;

            if (!LineQualifies(point) || IsPipeEscaped(point))
                return;

            int lineNumber = point.GetContainingLine().LineNumber;

            List<ITextSnapshotLine> textLines = view.TextBuffer.CurrentSnapshot.Lines.ToList();

            int tableStartIndex = textLines
                .Select((text, index) => new { text, index })
                .Where(a => a.index <= lineNumber)
                .OrderByDescending(a => a.index)
                .TakeWhile(a => a.text.GetText().TrimStart().StartsWith("|"))
                .Min(a => a.index);

            int tableEndIndex = textLines
                .Select((text, index) => new { text, index })
                .Where(a => a.index >= lineNumber)
                .OrderBy(a => a.index)
                .TakeWhile(a => a.text.GetText().TrimStart().StartsWith("|"))
                .Max(a => a.index);

            if (tableStartIndex == tableEndIndex)
                return;

            List<ITextSnapshotLine> lines = textLines.Skip(tableStartIndex).Take(tableEndIndex - tableStartIndex + 1).ToList();

            List<List<Tuple<Span, string>>> lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

            int maxColumns = lineColumns.Max(a => a.Count);

            view.Caret.MoveToPreviousCaretPosition();
            ITextEdit edit = view.TextBuffer.CreateEdit();

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
            edit.Apply();
            view.Caret.MoveToNextCaretPosition();

            view.Caret.EnsureVisible();
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
                .TakeWhile(a => a == 92)
                .Count() % 2 == 1;
        }
    }
}

