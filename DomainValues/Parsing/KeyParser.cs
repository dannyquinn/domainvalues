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

            if (span.Text.Length > 4)
            {
                var param = source.GetTextSpan(span.Start+3);

                var matches = RegExpr.Variable.Matches(param.Text);

                foreach (Match match in matches)
                {
                    yield return new ParsedSpan(lineNumber, TokenType.Key | TokenType.Variable, param.Start + match.Index, match.Value);
                }

                var spanStart = -1;
                var spanRange = 1;

                for (int i = 0; i < param.Text.Length; i++)
                {
                    if (matches.Cast<Match>().Any(a => i >= a.Index && i <= a.Index + a.Length - 1) || !Extensions.WhitespacePredicate(param.Text[i]))
                    {
                        if (spanStart != -1)
                        {
                            yield return new ParsedSpan(lineNumber,TokenType.Parameter, param.Start+spanStart,param.Text.Substring(spanStart,spanRange),"Invalid text");
                            spanStart = -1;
                        }
                        continue;
                    }
                    if (spanStart == -1)
                    {
                        spanStart = i;
                        spanRange = 1;
                    }
                    else
                    {
                        spanRange++;
                    }
                }
                if (spanStart != -1)
                {
                    yield return new ParsedSpan(lineNumber, TokenType.Parameter, param.Start + spanStart, param.Text.Substring(spanStart, spanRange), "Invalid text");
                }

            }
        }

        protected override TokenType PrimaryType => TokenType.Key;
    }
}
