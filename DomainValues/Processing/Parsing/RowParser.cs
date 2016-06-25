using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class RowParser : ParserBase
    {
        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            List<ParsedSpan> parsedSpans = new List<ParsedSpan>();
            TextSpan span = source.GetTextSpan();

            TokenType token = TokenType.HeaderRow;

            if (expectedType != null && (expectedType & TokenType.HeaderRow) != 0)
            {
                NextType = TokenType.ItemRow;
            }
            else
            {
                token = TokenType.ItemRow;
                NextType = TokenType.Data | TokenType.Table | TokenType.ItemRow;
            }

            int lastPipe = Regex.Matches(span.Text, @"(?<!\\)\|", RegexOptions.Compiled).Cast<Match>().Last().Index + 1;

            ParsedSpan data = new ParsedSpan(lineNumber, token, span.To(lastPipe));

            CheckKeywordOrder(data, expectedType);

            parsedSpans.Add(data);

            if (source.Length > lastPipe)
            {
                TextSpan invalidSpan = span.From(lastPipe);

                if (invalidSpan.Text.Length > 0)
                    parsedSpans.Add(new ParsedSpan(lineNumber, TokenType.Parameter, invalidSpan, Errors.Invalid));
            }


            if (token == TokenType.ItemRow)
                return parsedSpans;

            MatchCollection columns = RegExpr.Columns.Matches(source);

            List<Match> duplicates = columns.Cast<Match>()
                .GroupBy(a => a.Value.Trim().ToLower())
                .SelectMany(a => a.Skip(1))
                .ToList();

            foreach (Match duplicate in duplicates)
            {
                TextSpan value = duplicate.Value.GetTextSpan();
                parsedSpans.Add(new ParsedSpan(lineNumber, TokenType.HeaderRow, duplicate.Index + value.Start, value.Text,string.Format(Errors.DuplicateValue,"Column",value.Text)));
            }
            return parsedSpans;
        }

        protected override TokenType PrimaryType => TokenType.HeaderRow | TokenType.ItemRow;

        protected override TokenType? NextType { get; set; }
    }
}
