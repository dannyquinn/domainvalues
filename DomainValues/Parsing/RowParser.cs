using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Parsing
{
    internal class RowParser : LineParser
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            TokenType token = TokenType.HeaderRow;

            if ((expectedTokenType & TokenType.HeaderRow) != 0)
            {
                NextTokenType = TokenType.ItemRow;
            }
            else
            {
                token = TokenType.ItemRow;
                NextTokenType = TokenType.Data | TokenType.Table | TokenType.ItemRow;
            }

            var span = new ParsedSpan(lineNumber,token,source.GetTextSpan());

            CheckOrder(span,expectedTokenType);

            yield return span;
        }

        internal override TokenType PrimaryType => TokenType.HeaderRow | TokenType.ItemRow;
    }
}
