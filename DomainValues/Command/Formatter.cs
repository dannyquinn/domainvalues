﻿using System;
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

            List<List<(Span span, string text)>> lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

            int maxColumns = lineColumns.Max(a => a.Count);

            lines
                .Select(a => new Span(a.Start, a.GetText().IndexOf('|')))
                .ToList()
                .ForEach(a => edit.Replace(a, new string(' ', 8)));

            for (int column = 0; column < maxColumns; column++)
            {
                List<List<(Span span, string text)>> cols = lineColumns.Where(a => a.Count >= column + 1).ToList();
                int maxColLen = cols.Max(a => a[column].text.Length);

                foreach (List<(Span span, string text)> col in cols)
                {
                    string currentText = col[column].text;

                    string newText = $" {currentText}{new string(' ', maxColLen - currentText.Length)} ";

                    edit.Replace(col[column].span, newText);
                }

            }
        }
        private static List<(Span span, string text)> GetColumns(SnapshotSpan span)
        {
            return RegExpr.Columns.Matches(span.GetText()).Cast<Match>()
                .Select(a => (new Span(span.Start + a.Index, a.Length), a.Value.Trim()))
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
