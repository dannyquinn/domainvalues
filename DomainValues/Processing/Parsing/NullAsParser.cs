using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class NullAsParser : ParserBase
    {
        internal override TokenType PrimaryType => TokenType.NullAs;

        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Table | TokenType.SpaceAs;

            var span = source.GetTextSpan();

            if (span.Text.Length > 7 && span.Text.Substring(7, 1) != " ")
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span,Errors.INVALID);
                yield break;
            }

            var nullAs = new ParsedSpan(lineNumber, TokenType.NullAs, span.Start, span.Text.Substring(0, 7));

            if (span.Text.Length == 7 || string.IsNullOrWhiteSpace(span.Text.Substring(7)))
            {
                nullAs.Errors.Add(new Error("Null as expects a parameter.", false));
            }

            CheckOrder(nullAs, expectedTokenType);

            yield return nullAs;


            if (span.Text.Length > 7)
            {
                yield return new ParsedSpan(lineNumber, TokenType.NullAs | TokenType.Parameter, source.GetTextSpan(span.Start + 7));
            }
        }
    }
}
