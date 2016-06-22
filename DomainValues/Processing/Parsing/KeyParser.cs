using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class KeyParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Data | TokenType.Enum;

            var span = source.GetTextSpan();

            if (!IsValid(span, 3))
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span,Errors.Invalid);
                yield break;
            }

            var key = new ParsedSpan(lineNumber, TokenType.Key, span.Start, span.Text.Substring(0, 3));

            if (span.Text.Length == 3 || string.IsNullOrWhiteSpace(span.Text.Substring(4)))
            {
                key.Errors.Add(new Error(string.Format(Errors.ExpectsParams,"Key"), false));
            }

            CheckOrder(key, expectedTokenType);

            yield return key;

            if (span.Text.Length <= 4)
                yield break;

            var param = source.GetTextSpan(span.Start + 3);

            var matches = Regex.Matches(param.Text, @"\S+", RegexOptions.Compiled);

            var duplicates = matches.Cast<Match>().GroupBy(a => a.Value.ToLower()).SelectMany(a => a.Skip(1)).ToList();

            foreach (Match match in matches)
            {
                var spanVar = new ParsedSpan(lineNumber, TokenType.Key | TokenType.Variable, param.Start + match.Index, match.Value);

                if (duplicates.Contains(match))
                {
                    spanVar.Errors.Add(new Error(string.Format(Errors.DuplicateValue,"Key" ,match.Value), false));
                }
                yield return spanVar;
            }
        }

        internal override TokenType PrimaryType => TokenType.Key;
    }
}
