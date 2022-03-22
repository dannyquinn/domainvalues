using System.Collections.Generic;

namespace DomainValues.Shared.Model
{
    internal class ParsedSpan
    {
        public ParsedSpan(int lineNumber,TokenType type,int start,string text,string error=null,bool outputWindowOnly=false)
        {
            LineNumber = lineNumber;
            Type = type;
            Start = start;
            Text = text;
            Errors = new List<Error>();

            if (error!=null)
                Errors.Add(new Error(error,outputWindowOnly)); 
        }

        public ParsedSpan(int lineNumber, TokenType type, TextSpan span, string error = null,bool outputWindowOnly=false)
            : this(lineNumber, type, span.Start, span.Text, error,outputWindowOnly)
        {
            
        }
        public int LineNumber { get; }
        public TokenType Type { get; }
        public int Start { get; }
        public string Text { get; }
        public List<Error> Errors { get; }
    }
}
