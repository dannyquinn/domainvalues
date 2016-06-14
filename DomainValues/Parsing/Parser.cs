using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Parsing
{
    internal static class Parser
    {
        public static List<ParsedSpan> GetSpans(string source, bool extendedCheck)
        {
            var spans = new List<ParsedSpan>();

            var lineCount = -1;

            TokenType? expectedType = TokenType.Table;

            using (var sr = new StringReader(source))
            {
                string currentLine;

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
                        spans.Add(new ParsedSpan(lineCount, TokenType.Parameter, currentLine.GetTextSpan(), "Invalid text in file"));
                        continue;
                    }

                    var parser = lookup.Value;

                    foreach (var span in parser.ParseLine(lineCount, currentLine, expectedType))
                    {
                        spans.Add(span);
                    }

                    expectedType = parser.NextTokenType;


                }
            }

            if (!spans.Any(a => a.Errors.Any()) && spans.Any(a => a.Type == TokenType.Table) && expectedType != (TokenType.Table | TokenType.ItemRow | TokenType.Data))
            {
                spans.Last(a=>a.Type!=TokenType.Comment).Errors.Add("Unexpected end of file");
            }

            //TODO - Extended validation
            return spans;
        }

        internal static Dictionary<string, LineParser> Rules = new Dictionary<string, LineParser>
        {
            {"#", new CommentParser()},
            {"table", new TableParser()},
            {"key", new KeyParser()},
            {"data", new DataParser()},
            {"|", new RowParser()}
        };
    }
}
