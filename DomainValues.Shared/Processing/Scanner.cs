﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing.Parsing;

namespace DomainValues.Shared.Processing
{
    internal static class Scanner
    {
        public static List<ParsedSpan> GetSpans(string source, bool validateBlocks)
        {
            var spans = new List<ParsedSpan>();

            var lineCount = -1;

            TokenType? expectedType = TokenType.Table | TokenType.NullAs | TokenType.SpaceAs | TokenType.CopySql;

            using (StringReader sr = new StringReader(source))
            {
                var currentLine = string.Empty;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    lineCount++;

                    if (string.IsNullOrWhiteSpace(currentLine))
                        continue;

                    var lookup = Rules.SingleOrDefault(a =>
                        currentLine.TrimStart().Length >= a.Key.Length &&
                        currentLine.TrimStart().Substring(0, a.Key.Length).Equals(a.Key, StringComparison.CurrentCultureIgnoreCase));

                    if (lookup.Key == null)
                    {
                        spans.Add(new ParsedSpan(lineCount, TokenType.Parameter, currentLine.GetTextSpan(), Errors.Invalid));
                        continue;
                    }

                    var parser = lookup.Value;

                    spans.AddRange(parser.ParseLine(lineCount, currentLine, expectedType));

                    expectedType = parser.NextExpectedToken;
                }
            }

            if (spans.Any(a => a.Type == TokenType.Table) && expectedType != (TokenType.Table | TokenType.ItemRow | TokenType.Data))
            {
                spans.Last(a => a.Type != TokenType.Comment).Errors.Add(new Error(Errors.EndOfFile, true));
            }

            if (validateBlocks)
            {
                Validate.CheckBlocks(spans);
            }

            return spans;
        }

        internal static Dictionary<string, ParserBase> Rules = new Dictionary<string, ParserBase>
        {
            {"#", new CommentParser()},
            {"table", new TableParser()},
            {"key", new KeyParser()},
            {"data", new DataParser()},
            {"|", new RowParser()},
            {"enum",new EnumParser() },
            {"template",new TemplateParser() },
            {"null as", new NullAsParser() },
            {"space as",new SpaceAsParser() },
            {"copy sql to",new CopySqlParser() }
        };
    }
}
