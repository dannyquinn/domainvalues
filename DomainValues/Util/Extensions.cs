using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using DomainValues.Model;

namespace DomainValues.Util
{
    internal static class Extensions
    {
        internal static Predicate<char> IsNotWhiteSpace = a => !(new[] { '\t', '\n', '\r', ' ' }).Any(c => a == c);
        internal static Span TrimedSpan(this string source)
        {
            return new Span(
                Array.FindIndex(source.ToCharArray(), IsNotWhiteSpace),
                Array.FindLastIndex(source.ToCharArray(), IsNotWhiteSpace) -
                Array.FindIndex(source.ToCharArray(),IsNotWhiteSpace) + 1
                );
        }

        internal static TextSpan GetTextSpan(this string source, int offset = 0)
        {
            var span = source.Substring(offset).TrimedSpan();

            return new TextSpan(span.Start + offset, source.Substring(span.Start + offset).Trim());
        }

        internal static IEnumerable<List<ParsedSpan>> GetStatementBlocks(this List<ParsedSpan> source)
        {
            var range = source
                .Where(a => a.Type == TokenType.Table)
                .Select(a => a.LineNumber)
                .ToList();

            range.Add(source.Max(a=>a.LineNumber)+1);

            for (int i = 0; i < range.Count - 1; i++)
            {
                var start = range[i];
                var end = range[i + 1];

                yield return source.Where(a => a.LineNumber >= start && a.LineNumber < end).ToList();
            }

        }

        internal static IEnumerable<Span> ToRange(this IEnumerable<int> source)
        {
            var e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                yield break;
            }

            var currentList = new List<int> {e.Current};

            while (e.MoveNext())
            {
                var current = e.Current;

                if (current.Equals(currentList.Last()+1))
                {
                    currentList.Add(current);
                }
                else
                {
                    yield return new Span(currentList.Min(),currentList.Max()-currentList.Min()+1);
                    currentList = new List<int> {current};
                }
            }
            yield return new Span(currentList.Min(), currentList.Max() - currentList.Min() + 1);
        }

        internal static IEnumerable<string> GetColumns(this string source)
        {
            return RegExpr.Columns.Matches(source).Cast<Match>()
                 .Select(a => a.Value
                    .Trim()
                    .Replace("\\\\\\|", "\\\0")
                    .Replace("\\|", "|")
                    .Replace("\\\0", "\\|")
                );
        }
    }
}
