using System.Collections.Generic;

namespace DomainValues.Model
{
    internal class ParsedSpan
    {
        public ParsedSpan(int lineNumber,TokenType type,int start,string text,string error=null)
        {
            LineNumber = lineNumber;
            Type = type;
            Start = start;
            Text = text;
            Errors = new List<string>();

            if (error!=null)
                Errors.Add(error);
        }

        public ParsedSpan(int lineNumber, TokenType type, TextSpan span, string error = null)
            : this(lineNumber, type, span.Start, span.Text, error)
        {
            
        }
        public int LineNumber { get; }
        public TokenType Type { get; }
        public int Start { get; }
        public string Text { get; }
        public List<string> Errors { get; }
    }
}
