using DomainValues.Shared.Common;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Command
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(AlignTable))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class AlignTable : IChainedCommandHandler<TypeCharCommandArgs>
    {
        public string DisplayName => nameof(AlignTable);

        public void ExecuteCommand(TypeCharCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            nextCommandHandler();

            if (args.TypedChar == '|')
            {
                FormatTable(args);
            }
        }

        private void FormatTable(TypeCharCommandArgs args)
        {
            var point = args.TextView.Caret.Position.BufferPosition;

            if (!LineQualifies(point) || IsPipeEscaped(point))
            {
                return;
            }

            args.TextView.Caret.MoveToPreviousCaretPosition();

            using (var edit = args.SubjectBuffer.CreateEdit())
            {
                var blockStart = point.GetContainingLine().LineNumber;

                var blockEnd = blockStart;

                while (blockStart > 0)
                {
                    var lineText = args.SubjectBuffer.CurrentSnapshot.GetLineFromLineNumber(--blockStart).GetText().TrimStart();

                    if (lineText.StartsWith("table", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                while (blockEnd < args.SubjectBuffer.CurrentSnapshot.LineCount - 1)
                {
                    var lineText = args.SubjectBuffer.CurrentSnapshot.GetLineFromLineNumber(++blockEnd).GetText().TrimStart();

                    if (lineText.StartsWith("table", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                AlignRows(edit, args.SubjectBuffer, blockStart, blockEnd);

                if (edit.HasEffectiveChanges)
                {
                    edit.Apply();
                }

                args.TextView.Caret.MoveToNextCaretPosition();

                args.TextView.Caret.EnsureVisible();
            }
        }

        public CommandState GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextCommandHandler)
        {
            return CommandState.Available;
        }

        private void AlignRows(ITextEdit edit, ITextBuffer buffer, int start, int end)
        {
            var lines = buffer.CurrentSnapshot.Lines
                .Where(a =>
                    a.LineNumber >= start &&
                    a.LineNumber <= end &&
                    a.GetText().TrimStart().StartsWith("|")
                )
                .ToList();

            var lineColumns = lines.Select(a => GetColumns(a.Extent)).ToList();

            var maxColumns = lineColumns.Max(a => a.Count);

            foreach (var line in lines)
            {
                string lineText = line.GetText();

                int firstPipe = lineText.IndexOf('|');

                string existingText = lineText.Substring(0, firstPipe);
                string wantedText = new string('\t', 2);

                if (existingText.Equals(wantedText))
                {
                    continue;
                }

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
            var lineText = point.GetContainingLine();

            return lineText
                       .GetText()
                       .Substring(0, point.Position - lineText.Extent.Span.Start - 1)
                       .Reverse()
                       .TakeWhile(a => a == 92) // 92 = \
                       .Count() % 2 == 1;
        }
    }
}
