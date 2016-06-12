using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using Microsoft.VisualStudio.Text.Differencing;

namespace DomainValues.Parsing
{
    internal abstract class LineParser
    {
        internal abstract IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType);
        protected abstract TokenType PrimaryType { get; }
        internal TokenType? NextTokenType { get; set; }

        protected void CheckOrder(ParsedSpan span, TokenType? expectedType)
        {
            if (expectedType == null)
                return;

            if ((expectedType & PrimaryType) == 0)
            {
                span.Errors.Add($"{span.Type} was unexpected.  Expected {expectedType}");
            }
        }

        protected bool IsValid(TextSpan span, int length) => span.Text.Length == length || span.Text.Substring(length, 1) == " ";
    }
}
