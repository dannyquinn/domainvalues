using System.Collections.Generic;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class DataParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.HeaderRow;

            var span = source.GetTextSpan();

            if (!IsValid(span, 4))
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span, "Invalid text in file.");
                yield break;
            }

            var data = new ParsedSpan(lineNumber, TokenType.Data, span.Start, span.Text.Substring(0, 4));

            CheckOrder(data, expectedTokenType);

            yield return data;

            if (span.Text.Length <= 4)
                yield break;

            var param = source.GetTextSpan(span.Start + 4);

            if (string.IsNullOrWhiteSpace(param.Text))
                yield break;

            yield return new ParsedSpan(lineNumber, TokenType.Parameter, param, "Data keyword does not have a parameter.");
        }

        internal override TokenType PrimaryType => TokenType.Data;
    }
}