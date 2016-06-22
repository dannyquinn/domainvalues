using System.Collections.Generic;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class TemplateParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Data;

            var span = source.GetTextSpan();

            if (!IsValid(span, 8))
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span, Errors.INVALID);
                yield break;
            }

            var template = new ParsedSpan(lineNumber, TokenType.Template, span.Start, span.Text.Substring(0, 8));

            if (span.Text.Length == 8 || string.IsNullOrWhiteSpace(span.Text.Substring(8)))
            {
                template.Errors.Add(new Error("Template expects at least one parameter, the dataitem to use as the enum member.", false));
            }

            CheckOrder(template, expectedTokenType);

            yield return template;

            if (span.Text.Length <= 8)
                yield break;


            var patterns = new Dictionary<string, int>
            {
                {@"^\s*\[(\w+)\]\s+(\w+)\s*=\s*(\w+)\s*$", 0},
                {@"^\s*\[(\w+)\]\s+(\w+)\s*$", 1},
                {@"^\s*(\w+)\s*$", 2},
                {@"^\s*(\w+)\s*=\s*(\w+)\s*$", 3}
            };

            var param = source.GetTextSpan(span.Start + 8);

            bool matched = false;
            foreach (var pattern in patterns)
            {
                var match = Regex.Match(param.Text, pattern.Key, RegexOptions.Compiled);

                matched = match.Success;

                if (!matched)
                    continue;

                switch (pattern.Value)
                {
                    case 0:
                        yield return new ParsedSpan(lineNumber, TokenType.EnumDesc, param.Start + match.Groups[1].Index, match.Groups[1].Value);
                        yield return new ParsedSpan(lineNumber, TokenType.EnumMember, param.Start + match.Groups[2].Index, match.Groups[2].Value);
                        yield return new ParsedSpan(lineNumber, TokenType.EnumInit, param.Start + match.Groups[3].Index, match.Groups[3].Value);
                        break;
                    case 1:
                        yield return new ParsedSpan(lineNumber, TokenType.EnumDesc, param.Start + match.Groups[1].Index, match.Groups[1].Value);
                        yield return new ParsedSpan(lineNumber, TokenType.EnumMember, param.Start + match.Groups[2].Index, match.Groups[2].Value);
                        break;
                    case 2:
                        yield return new ParsedSpan(lineNumber, TokenType.EnumMember, param.Start + match.Groups[1].Index, match.Groups[1].Value);
                        break;
                    case 3:
                        yield return new ParsedSpan(lineNumber, TokenType.EnumMember, param.Start + match.Groups[1].Index, match.Groups[1].Value);
                        yield return new ParsedSpan(lineNumber, TokenType.EnumInit, param.Start + match.Groups[2].Index, match.Groups[2].Value);
                        break;
                }
                break;
            }
            if (!matched)
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, param, "Cannot determine meaning from string.");
            }
        }
        
        internal override TokenType PrimaryType => TokenType.Template;
    }
}
