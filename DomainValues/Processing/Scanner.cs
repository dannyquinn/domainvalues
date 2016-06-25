using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DomainValues.Model;
using DomainValues.Processing.Parsing;
using DomainValues.Util;

namespace DomainValues.Processing
{
    internal static class Scanner
    {
        public static List<ParsedSpan> GetSpans(string source, bool validateBlocks)
        {
            List<ParsedSpan> spans = new List<ParsedSpan>();

            int lineCount = -1;

            TokenType? expectedType = TokenType.Table | TokenType.NullAs | TokenType.SpaceAs;

            using (StringReader sr = new StringReader(source))
            {
                string currentLine;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    lineCount++;

                    if (string.IsNullOrWhiteSpace(currentLine))
                        continue;

                    KeyValuePair<string, ParserBase> lookup = Rules.SingleOrDefault(a =>
                        currentLine.TrimStart().Length >= a.Key.Length &&
                        currentLine.TrimStart().Substring(0, a.Key.Length).Equals(a.Key, StringComparison.CurrentCultureIgnoreCase));

                    if (lookup.Key == null)
                    {
                        spans.Add(new ParsedSpan(lineCount, TokenType.Parameter, currentLine.GetTextSpan(), Errors.Invalid));
                        continue;
                    }

                    ParserBase parser = lookup.Value;

                    spans.AddRange(parser.ParseLine(lineCount, currentLine,expectedType));

                    expectedType = parser.NextExpectedToken;
                }
            }

            if (spans.Any(a => a.Type == TokenType.Table) && expectedType != (TokenType.Table | TokenType.ItemRow | TokenType.Data))
            {
                spans.Last(a => a.Type != TokenType.Comment).Errors.Add(new Error(Errors.EndOfFile, true));
            }

            if (validateBlocks)
                Validate.CheckBlocks(spans);

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
            {"space as",new SpaceAsParser() }
        };
    }
}
