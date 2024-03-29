﻿using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class KeyParser : ParserBase
    {
        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            var parameters = Regex.Matches(span.Text, @"\S+")
                .Cast<Match>()
                .Select(a => new TextSpan(span.Start + a.Index, a.Value))
                .ToList();

            var duplicates = parameters
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1))
                .ToList();

            foreach (var parameter in parameters)
            {
                var parsedSpan = new ParsedSpan(lineNumber,TokenType.Key | TokenType.Parameter,parameter);

                if (duplicates.Any(a=>a.Start==parameter.Start && a.Text == parameter.Text)) 
                {
                    parsedSpan.Errors.Add(new Error(string.Format(Errors.DuplicateValue,"Key",parameter.Text)));
                }

                yield return parsedSpan;
            }
        }

        protected override TokenType PrimaryType => TokenType.Key;
        protected override TokenType? NextType { get; set; } = TokenType.Data | TokenType.Enum;
        protected override int KeywordLength => 3;
    }
}
