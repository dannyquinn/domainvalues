using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class EnumParser : ParserBase
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = TokenType.Template;

            var span = source.GetTextSpan();

            if (!IsValid(span, 4))
            {
                yield return new ParsedSpan(lineNumber, TokenType.Parameter, span, "Invalid text in file.");
                yield break;
            }

            var enu = new ParsedSpan(lineNumber, TokenType.Enum, span.Start, span.Text.Substring(0, 4));

            if (span.Text.Length == 4 || string.IsNullOrWhiteSpace(span.Text.Substring(4)))
            {
                enu.Errors.Add(new Error("Enum expects at least parameter, the name of the enumeration", false));
            }

            CheckOrder(enu, expectedTokenType);

            if (span.Text.Length <= 4)
            {
                yield return enu;
                yield break;
            }

            var param = source.GetTextSpan(span.Start + 4);

            var matches = Regex.Matches(param.Text, @"\w+").Cast<Match>().Select(a => new TextSpan(a.Index, a.Value));

            var allowed = new Dictionary<TokenType, List<string>>()
            {
                {TokenType.AccessType, new List<string> {"public", "internal"}},
                {TokenType.BaseType, new List<string> {"byte", "sbyte", "short", "int16", "ushort", "int", "int32", "uint", "long", "int64", "ulong"}},
                {TokenType.FlagsAttribute, new List<string> {"flags"}}
            };

            TokenType flags = TokenType.AccessType | TokenType.BaseType | TokenType.FlagsAttribute | TokenType.Parameter;

            foreach (var match in matches)
            {
                bool found = false;
                foreach (var type in allowed)
                {
                    if (type.Value.Contains(match.Text.ToLower()))
                    {
                        found = true;
                        if ((flags & type.Key) == 0)
                        {
                            yield return new ParsedSpan(lineNumber, type.Key, match.Start + param.Start, match.Text, $"Already found a parameter that looks like the enum {type.Key}");
                            continue;
                        }
                        flags = flags ^ type.Key;

                        yield return new ParsedSpan(lineNumber, type.Key, match.Start + param.Start, match.Text);
                        break;
                    }
                }
                if (found)
                    continue;

                if ((flags & TokenType.Parameter) == 0)
                {
                    yield return new ParsedSpan(lineNumber, TokenType.Enum | TokenType.Parameter, match.Start + param.Start, match.Text, "Invalid Text.");
                    continue;
                }
                flags = flags ^ TokenType.Parameter;
                yield return new ParsedSpan(lineNumber, TokenType.Enum | TokenType.Parameter, match.Start + param.Start, match.Text);
            }
            if ((flags & TokenType.Parameter) != 0)
                enu.Errors.Add(new Error("No name provided for enumeration.", false));

            yield return enu;
        }

        internal override TokenType PrimaryType => TokenType.Enum;
    }
}
