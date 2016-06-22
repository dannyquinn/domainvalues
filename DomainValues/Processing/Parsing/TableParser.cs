using System.Collections.Generic;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class TableParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Key;

            var span = source.GetTextSpan();

            if (span.Text.Length > 5 && span.Text.Substring(5, 1) != " ")
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span,Errors.Invalid);
                yield break;
            }

            var table = new ParsedSpan(lineNumber, TokenType.Table, span.Start, span.Text.Substring(0, 5));

            if (span.Text.Length == 5 || string.IsNullOrWhiteSpace(span.Text.Substring(6)))
            {
                table.Errors.Add(new Error(string.Format(Errors.ExpectsParam,"Table"), false));
            }

            CheckOrder(table, expectedTokenType);

            yield return table;

            if (span.Text.Length > 6)
            {
                yield return new ParsedSpan(lineNumber, TokenType.Table | TokenType.Parameter, source.GetTextSpan(span.Start + 5));
            }
        }

        internal override TokenType PrimaryType => TokenType.Table;
    }
}