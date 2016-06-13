using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;


namespace DomainValues.Parsing
{
    internal class KeyParser : LineParser
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Data;

            var span = source.GetTextSpan();

           if (!IsValid(span,3))
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span, "Invalid text in file");
                yield break;
            }

           var key = new ParsedSpan(lineNumber,TokenType.Key,span.Start,span.Text.Substring(0,3));

            if (span.Text.Length == 3 || string.IsNullOrWhiteSpace(span.Text.Substring(4)))
            {
                key.Errors.Add("Key expects at least one parameter");
            }

            CheckOrder(key,expectedTokenType);

            yield return key;

            if (span.Text.Length <= 4)
                yield break;

            var param = source.GetTextSpan(span.Start+3);

            var matches = RegExpr.Variable.Matches(param.Text);

            foreach (Match match in matches)
            {
                yield return new ParsedSpan(lineNumber, TokenType.Key | TokenType.Variable, param.Start + match.Index, match.Value);
            }

            var invalidSpans = param.Text.ToCharArray()
                .Select((a, i) => new
                {
                    Valid = matches.Cast<Match>().SelectMany(b => Enumerable.Range(b.Index, b.Length)).Contains(i),
                    Char = a,
                    Index = i
                })
                .Where(a => !a.Valid && Extensions.WhitespacePredicate(a.Char))
                .Select(a => a.Index)
                .ToRange();

            foreach (var invalidSpan in invalidSpans)
            {
                yield return new ParsedSpan(lineNumber,TokenType.Parameter,param.Start+invalidSpan.Start,param.Text.Substring(invalidSpan.Start,invalidSpan.Length),"Invalid text");
            }
        }

        internal override TokenType PrimaryType => TokenType.Key;
    }
}
