using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class EnumParser : ParserBase
    {
        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            var spans =  base.ParseLine(lineNumber, source,expectedType);

            if (spans.Count > 1 && spans.All(a => a.Type != (TokenType.Enum | TokenType.Parameter)))
            {
                spans.Single(a=>a.Type==PrimaryType).Errors.Add(new Error(Errors.EnumNoName));
            }

            return spans;
        }

        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            var parameters = Regex.Matches(span.Text, @"\S+")
                .Cast<Match>()
                .Select(a => new TextSpan(a.Index+span.Start, a.Value));

            var knownTokens = new Dictionary<TokenType, List<string>>
            {
                {TokenType.AccessType, new List<string> {"public", "internal"}},
                {TokenType.BaseType, new List<string> {"byte", "sbyte", "short", "int16", "ushort", "int", "int32", "uint", "long", "int64", "ulong"}},
                {TokenType.FlagsAttribute, new List<string> {"flags"}}
            };

            var flags = TokenType.AccessType | TokenType.BaseType | TokenType.FlagsAttribute | TokenType.Parameter;

            foreach (var parameter in parameters)
            {
                var found = false;

                foreach (var type in knownTokens)
                {
                    if (!type.Value.Contains(parameter.Text, StringComparer.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }

                    found = true;

                    var parsedSpan = new ParsedSpan(lineNumber,type.Key,parameter);

                    if ((flags & type.Key) == 0)
                    {
                        parsedSpan.Errors.Add(new Error(string.Format(Errors.EnumDuplicate, type.Key)));
                    }
                    else
                    {
                        flags ^= type.Key;
                    }

                    yield return parsedSpan;

                    break;
                }
                if (found)
                {
                    continue;
                }

                var paramSpan = new ParsedSpan(lineNumber,TokenType.Enum | TokenType.Parameter,parameter);

                if ((flags & TokenType.Parameter) == 0)
                {
                    paramSpan.Errors.Add(new Error(Errors.Invalid));
                }
                else
                {
                    flags ^= TokenType.Parameter;
                }

                yield return paramSpan;
            }
        }

        protected override TokenType PrimaryType => TokenType.Enum;
        protected override TokenType? NextType { get; set; } = TokenType.Template;
        protected override int KeywordLength => 4;
    }
}
