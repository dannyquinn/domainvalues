using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            var lastPipe = RegExpr.LastPipe.Matches(source).Cast<Match>().Last().Index+1;

            var span = new ParsedSpan(lineNumber,token,source.Substring(0,lastPipe).GetTextSpan());

            CheckOrder(span,expectedTokenType);

            yield return span;

            if (source.Length > lastPipe)
            {
                var invalidSpan = source.GetTextSpan(lastPipe);

                if (invalidSpan.Text.Length>0)
                    yield return new ParsedSpan(lineNumber, TokenType.Parameter, invalidSpan, "Invalid text");
            }
                

            if (token==TokenType.ItemRow)
                yield break;

            var columns = RegExpr.Columns.Matches(source);

            var duplicates = columns.Cast<Match>()
                .GroupBy(a => a.Value.Trim().ToLower())
                .SelectMany(a => a.Skip(1))
                .ToList();

            foreach (var duplicate in duplicates)
            {
                var value = duplicate.Value.GetTextSpan();
                yield return new ParsedSpan(lineNumber,TokenType.HeaderRow,duplicate.Index+value.Start,value.Text,$"Column {value.Text} is a duplicate value");
            }
        }

        internal override TokenType PrimaryType => TokenType.HeaderRow | TokenType.ItemRow;
    }
}
