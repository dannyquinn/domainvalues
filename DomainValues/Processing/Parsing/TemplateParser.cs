//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using DomainValues.Model;
//using DomainValues.Util;

//namespace DomainValues.Processing.Parsing
//{
//    internal class TemplateParser : ParserBase
//    {
//        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
//        {
//            Dictionary<string, int> patterns = new Dictionary<string, int>
//            {
//                {@"^\s*\[(\w+)\]\s*(\w+)\s*=\s*(\w+)\s*$", 0},
//                {@"^\s*\[(\w+)\]\s*(\w+)(\s*)$", 1},
//                {@"^(\s*)(\w+)\s*=\s*(\w+)\s*$", 2},
//                {@"^(\s*)(\w+)(\s*)$", 3}
//            };

//            foreach (KeyValuePair<string, int> pattern in patterns)
//            {
//                Match match = Regex.Match(span.Text, pattern.Key, RegexOptions.Compiled);

//                if (!match.Success)
//                    continue;

//                if (pattern.Value < 2)
//                    yield return new ParsedSpan(lineNumber, TokenType.EnumDesc, span.Start + match.Groups[1].Index, match.Groups[1].Value);

//                yield return new ParsedSpan(lineNumber, TokenType.EnumMember, span.Start + match.Groups[2].Index, match.Groups[2].Value);

//                if (pattern.Value%2 == 0)
//                    yield return new ParsedSpan(lineNumber, TokenType.EnumInit, span.Start + match.Groups[3].Index, match.Groups[3].Value);

//                yield break;
//            }
//            yield return new ParsedSpan(lineNumber, TokenType.Parameter, span, Errors.TemplatePatternNotRecognised);
//        }

//        protected override TokenType PrimaryType => TokenType.Template;
//        protected override TokenType? NextType { get; set; } = TokenType.Data;
//        protected override int KeywordLength => 8;
//    }
//}
