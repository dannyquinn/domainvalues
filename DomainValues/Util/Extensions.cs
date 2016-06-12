using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
