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

            if (extendedCheck)
                ExtendedChecks(spans);

            return spans;
        }

        internal static void ExtendedChecks(List<ParsedSpan> spans)
        {
            foreach (var block in spans.GetStatementBlocks())
            {
                var header = block.FirstOrDefault(a => a.Type == TokenType.HeaderRow);

                if (header == null)
                    continue;

                var columns = header.Text.GetColumns()
                    .Select(a=>a.ToLower())
                    .ToList();

                //TODO - Check Headers Match 
                //TODO - Check ItemRow columns equal HeaderRowColumns                

                CheckKeyVariables(columns, block.Where(a => a.Type == (TokenType.Key | TokenType.Variable)).ToList());
            }
        }

        internal static void CheckKeyVariables(List<string> columns, List<ParsedSpan> keyVars)
        {
            if (keyVars == null)
                return;

            foreach (var key in keyVars)
            {
                var keyValue = key.Text
                    .Substring(1, key.Text.Length - 2)
                    .ToLower();

                if (keyValue.EndsWith("*"))
                    keyValue = $"{keyValue}*";

                if (columns.Contains($"{keyValue}*"))
                {
                    key.Errors.Add($"Key value {key.Text} is marked as non db in the column row.  Cannot be used as a key.");
                    continue;
                }

                if (columns.Contains(keyValue))
                    continue;
                
                key.Errors.Add($"Key value {key.Text} not found in the column row.");
            }
            
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
