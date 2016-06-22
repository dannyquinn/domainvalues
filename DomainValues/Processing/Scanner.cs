﻿using System;
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
        public static List<ParsedSpan> GetSpans(string source, bool extendedCheck)
        {
            var spans = new List<ParsedSpan>();

            var lineCount = -1;

            TokenType? expectedType = TokenType.Table | TokenType.NullAs | TokenType.SpaceAs;

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
                        spans.Add(new ParsedSpan(lineCount, TokenType.Parameter, currentLine.GetTextSpan(), Errors.Invalid));
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

            if (spans.Any(a => a.Type == TokenType.Table) && expectedType != (TokenType.Table | TokenType.ItemRow | TokenType.Data))
            {
                spans.Last(a => a.Type != TokenType.Comment).Errors.Add(new Error(Errors.EndOfFile, true));
            }

            if (extendedCheck)
                ExtendedChecks(spans);

            return spans;
        }

        internal static void ExtendedChecks(List<ParsedSpan> spans)
        {
            CheckNullAsSpaceAs(spans);

            foreach (var block in spans.GetStatementBlocks())
            {
                var header = block.FirstOrDefault(a => a.Type == TokenType.HeaderRow);

                if (header == null)
                    continue;

                var columns = header.Text.GetColumns()
                    .Select(a => a.ToLower())
                    .ToList();

                CheckRowLengths(block);

                CheckKeyVariables(columns, block.Where(a => a.Type == (TokenType.Key | TokenType.Variable)));
                CheckEnumVariables(columns,block.Where(a=>(a.Type & (TokenType.EnumMember | TokenType.EnumDesc | TokenType.EnumInit))!=0));
            }

            CheckDuplicateTableNames(spans);
            CheckDuplicateEnumNames(spans);
        }

        private static void CheckNullAsSpaceAs(List<ParsedSpan> spans)
        {
            var nullAs = spans.FirstOrDefault(a => a.Type == (TokenType.NullAs | TokenType.Parameter));

            if (nullAs == null)
                return;

            var spaceAs = spans.FirstOrDefault(a => a.Type == (TokenType.SpaceAs | TokenType.Parameter));

            if (spaceAs == null)
                return;

            if (nullAs.Text.Equals(spaceAs.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                spaceAs.Errors.Add(new Error(Errors.NullAsSpaceAs,false));
            }
        }
        private static void CheckDuplicateEnumNames(IEnumerable<ParsedSpan> spans)
        {
            var duplicateEnumNames = spans
                .Where(a => a.Type == (TokenType.Enum | TokenType.Parameter))
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1));

            foreach (var duplicateEnumName in duplicateEnumNames)
            {
                duplicateEnumName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed,"Enum",duplicateEnumName.Text),false));
            }
        }
        private static void CheckDuplicateTableNames(IEnumerable<ParsedSpan> spans)
        {
            var duplicateTableNames = spans
                .Where(a => a.Type == (TokenType.Table | TokenType.Parameter))
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1));

            foreach (var duplicateTableName in duplicateTableNames)
            {
                duplicateTableName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed,"Table",duplicateTableName.Text), false));
            }
        }

        internal static void CheckRowLengths(List<ParsedSpan> spans)
        {
            var itemRows = spans
                .Where(a => a.Type == TokenType.HeaderRow || a.Type == TokenType.ItemRow)
                .OrderBy(a => a.LineNumber);

            var firstHeader = itemRows.First();

            var pipeCount = firstHeader.Text.ToCharArray().Count(a => a == '|');

            foreach (var itemRow in itemRows.Skip(1).Where(a => !a.Errors.Any()))
            {
                var rowPipeCount = itemRow.Text.ToCharArray().Count(a => a == '|');

                if (rowPipeCount != pipeCount)
                    itemRow.Errors.Add(new Error(Errors.RowCountMismatch, false));
            }
        }
        internal static void CheckKeyVariables(List<string> columns, IEnumerable<ParsedSpan> keyVars)
        {
            if (keyVars == null)
                return;

            foreach (var key in keyVars)
            {
                var keyValue = key.Text.ToLower();

                if (keyValue.EndsWith("*"))
                    keyValue = $"{keyValue}*";

                if (columns.Contains($"{keyValue}*"))
                {
                    key.Errors.Add(new Error(string.Format(Errors.KeyMapsToNonDBColumn,key.Text), false));
                    continue;
                }

                if (columns.Contains(keyValue))
                    continue;

                key.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns,"Key",key.Text), false));
            }
        }
        internal static void CheckEnumVariables(List<string> columns, IEnumerable<ParsedSpan> enumVars)
        {
            if (enumVars == null)
                return;

            foreach (var enumVar in enumVars)
            {
                var enumValue = enumVar.Text.ToLower();

                if (columns.Select(a => a.TrimEnd('*')).Contains(enumValue))
                    continue;

                enumVar.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns, "Template", enumVar.Text),false));
            }
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
