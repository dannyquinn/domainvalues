using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class RowParser : ParserBase
    {
        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            var parsedSpans = new List<ParsedSpan>();
            var span = source.GetTextSpan();

            var token = TokenType.HeaderRow;

            if (expectedType != null && (expectedType & TokenType.HeaderRow) != 0)
            {
                NextType = TokenType.ItemRow;
            }
            else
            {
                token = TokenType.ItemRow;
                NextType = TokenType.Data | TokenType.Table | TokenType.ItemRow;
            }

            var lastPipe = Regex.Matches(span.Text, @"(?<!\\)\|", RegexOptions.Compiled).Cast<Match>().Last().Index + 1;

            var data = new ParsedSpan(lineNumber, token, span.To(lastPipe));

            CheckKeywordOrder(data, expectedType);

            parsedSpans.Add(data);

            if (source.Length > lastPipe)
            {
                var invalidSpan = span.From(lastPipe);

                if (invalidSpan.Text.Length > 0)
                {
                    parsedSpans.Add(new ParsedSpan(lineNumber, TokenType.Parameter, invalidSpan, Errors.Invalid));
                }
            }


            if (token == TokenType.ItemRow)
            {
                return parsedSpans;
            }

            var columns = Extensions.Columns.Matches(source);

            var duplicates = columns.Cast<Match>()
                .GroupBy(a => a.Value.Trim().ToLower())
                .SelectMany(a => a.Skip(1))
                .ToList();

            foreach (var duplicate in duplicates)
            {
                var value = duplicate.Value.GetTextSpan();

                parsedSpans.Add(new ParsedSpan(lineNumber, TokenType.HeaderRow, duplicate.Index + value.Start, value.Text,string.Format(Errors.DuplicateValue,"Column",value.Text)));
            }
            return parsedSpans;
        }

        protected override TokenType PrimaryType => TokenType.HeaderRow | TokenType.ItemRow;

        protected override TokenType? NextType { get; set; }
    }
}
