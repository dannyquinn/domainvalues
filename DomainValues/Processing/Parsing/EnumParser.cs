using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class EnumParser : ParserBase
    {
        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            List<ParsedSpan> spans =  base.ParseLine(lineNumber, source,expectedType);

            if (spans.Count > 1 && spans.All(a => a.Type != (TokenType.Enum | TokenType.Parameter)))
            {
                spans.Single(a=>a.Type==PrimaryType).Errors.Add(new Error(Errors.EnumNoName));
            }
            return spans;
        }

        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            IEnumerable<TextSpan> parameters = Regex.Matches(span.Text, @"\S+")
                .Cast<Match>()
                .Select(a => new TextSpan(a.Index+span.Start, a.Value));

            Dictionary<TokenType, List<string>> knownTokens = new Dictionary<TokenType, List<string>>
            {
                {TokenType.AccessType, new List<string> {"public", "internal"}},
                {TokenType.BaseType, new List<string> {"byte", "sbyte", "short", "int16", "ushort", "int", "int32", "uint", "long", "int64", "ulong"}},
                {TokenType.FlagsAttribute, new List<string> {"flags"}}
            };

            TokenType flags = TokenType.AccessType | TokenType.BaseType | TokenType.FlagsAttribute | TokenType.Parameter;

            foreach (TextSpan parameter in parameters)
            {
                bool found = false;

                foreach (KeyValuePair<TokenType, List<string>> type in knownTokens)
                {
                    if (!type.Value.Contains(parameter.Text, StringComparer.CurrentCultureIgnoreCase))
                        continue;

                    found = true;

                    ParsedSpan parsedSpan = new ParsedSpan(lineNumber,type.Key,parameter);

                    if ((flags & type.Key) == 0)
                    {
                        parsedSpan.Errors.Add(new Error(string.Format(Errors.EnumDuplicate, type.Key)));
                    }
                    else
                    {
                        flags = flags ^ type.Key;
                    }
                    yield return parsedSpan;

                    break;
                }
                if (found)
                    continue;

                ParsedSpan paramSpan = new ParsedSpan(lineNumber,TokenType.Enum | TokenType.Parameter,parameter);
                if ((flags & TokenType.Parameter) == 0)
                {
                    paramSpan.Errors.Add(new Error(Errors.Invalid));
                }
                else
                {
                    flags = flags ^ TokenType.Parameter;
                }
                yield return paramSpan;
            }
        }

        protected override TokenType PrimaryType => TokenType.Enum;
        protected override TokenType? NextType { get; set; } = TokenType.Template;
        protected override bool HasParams => true;
        protected override int KeywordLength => 4;
    }
}
