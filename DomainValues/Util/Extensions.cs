using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using DomainValues.Model;

namespace DomainValues.Util
{
    internal static class Extensions
    {
        internal static Predicate<char> WhitespacePredicate = a => !(new[] { '\t', '\n', '\r', ' ' }).Any(c => a == c);
        internal static Span TrimedSpan(this string source)
        {
            return new Span(
                Array.FindIndex(source.ToCharArray(), WhitespacePredicate),
                Array.FindLastIndex(source.ToCharArray(), WhitespacePredicate) -
                Array.FindIndex(source.ToCharArray(),WhitespacePredicate) + 1
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
    }
}
