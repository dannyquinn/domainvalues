using System.Collections.Generic;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal abstract class ParserBase
    {
        internal abstract IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType);
        internal abstract TokenType PrimaryType { get; }
        internal TokenType? NextTokenType { get; set; }

        protected void CheckOrder(ParsedSpan span, TokenType? expectedType)
        {
            if (expectedType == null)
                return;

            if ((expectedType & PrimaryType) != 0)
                return;

            span.Errors.Add(new Error(string.Format(Errors.UnexpectedKeyword,span.Type,expectedType), false));
            NextTokenType = null;
        }

        protected bool IsValid(TextSpan span, int length) => span.Text.Length == length || span.Text.Substring(length, 1) == " ";
    }
}
