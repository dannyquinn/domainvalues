using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class KeyParser : ParserBase
    {
        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            List<TextSpan> parameters = Regex.Matches(span.Text, @"\S+")
                .Cast<Match>()
                .Select(a => new TextSpan(span.Start + a.Index, a.Value))
                .ToList();

            List<TextSpan> duplicates = parameters
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1))
                .ToList();

            foreach (TextSpan parameter in parameters)
            {
                ParsedSpan parsedSpan = new ParsedSpan(lineNumber,TokenType.Key | TokenType.Parameter,parameter);

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
