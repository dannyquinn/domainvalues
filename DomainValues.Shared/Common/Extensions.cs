using DomainValues.Shared.Model;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Common
{
    public static class Extensions
    {
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

        internal static TextSpan GetTextSpan(this string source) => new TextSpan(source);

        internal static Regex Columns = new Regex(@"(?<=(^|[^\\])\|)[^\|\\]*(?:\\.[^\|\\]*)*(?=\|)", RegexOptions.Compiled);

        internal static IEnumerable<List<ParsedSpan>> GetStatementBlocks(this List<ParsedSpan> source)
        {
            if (!source.Any())
                yield break;

            List<int> range = source
                .Where(a => a.Type == TokenType.Table)
                .Select(a => a.LineNumber)
                .ToList();

            range.Add(source.Max(a => a.LineNumber) + 1);

            for (int i = 0; i < range.Count - 1; i++)
            {
                int start = range[i];
                int end = range[i + 1];

                yield return source.Where(a => a.LineNumber >= start && a.LineNumber < end).ToList();
            }

        }

        internal static IEnumerable<string> GetColumns(this string source)
        {
            return Extensions.Columns.Matches(source).Cast<Match>()
                 .Select(a => a.Value
                    .Trim()
                    .Replace("\\\\\\|", "\\\0")
                    .Replace("\\|", "|")
                    .Replace("\\\0", "\\|")
                );
        }
    }
}
