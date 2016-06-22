using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class SpaceAsParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Table | TokenType.NullAs | TokenType.SpaceAs;

            var span = source.GetTextSpan();

            if (span.Text.Length > 8 && span.Text.Substring(8, 1) != " ")
            {
                yield return new ParsedSpan(lineNumber,TokenType.Parameter, span,"Invalid test in file.");
                yield break;
            }

            var spaceAs = new ParsedSpan(lineNumber,TokenType.SpaceAs,span.Start,span.Text.Substring(0,8));

            if (span.Text.Length == 8 || string.IsNullOrWhiteSpace(span.Text.Substring(8)))
            {
                spaceAs.Errors.Add(new Error("Space as expects a parameter",false));
            }

            CheckOrder(spaceAs,expectedTokenType);

            yield return spaceAs;

            if (span.Text.Length > 8)
            {
                yield return new ParsedSpan(lineNumber,TokenType.SpaceAs | TokenType.Parameter,source.GetTextSpan(span.Start+8));
            }
        }

        internal override TokenType PrimaryType => TokenType.SpaceAs;
    }
}
