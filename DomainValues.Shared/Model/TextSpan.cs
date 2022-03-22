using System;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace DomainValues.Shared.Model
{
    internal class TextSpan
    {
        public TextSpan(string text) : this(0, text)
        {
        }
        public TextSpan(int start,string text)
        {
            var span = GetExtent(text);
            Start = start+span.Start;
            Text = text.Substring(span.Start,span.Length);
        }
        public int Start { get; }
        public string Text { get; }
        public int Length => Text.Length;

        public TextSpan From(int index)
        {
            var span = GetExtent(Text.Substring(index));

            return new TextSpan(Start+span.Start+index,Text.Substring(span.Start+index));
        }

        public TextSpan To(int length)
        {
            var span = GetExtent(Text.Substring(0, length));

            return new TextSpan(Start,Text.Substring(span.Start,span.Length));
        }

        private Span GetExtent(string text)
        {
            bool charPredicate(char a) => !(new[] { '\t', '\r', '\n', ' ' }).Any(c => c == a);

            var chars = text.ToCharArray();

            var start = Array.FindIndex(chars, charPredicate);

            if (start == -1)
                return new Span(0, 0);

            var end = Array.FindLastIndex(chars, charPredicate);

            return new Span(start,end-start+1);
        }
    }
}
