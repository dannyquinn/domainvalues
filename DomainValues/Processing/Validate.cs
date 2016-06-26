using System;
using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing
{
    internal static class Validate
    {
        internal static void CheckBlocks(List<ParsedSpan> spans)
        {
            CheckNullAsSpaceAs(spans);

            foreach (List<ParsedSpan> block in spans.GetStatementBlocks())
            {
                ParsedSpan header = block.FirstOrDefault(a => a.Type == TokenType.HeaderRow);

                if (header == null)
                    continue;

                List<string> columns = header.Text.GetColumns()
                    .Select(a => a.ToLower())
                    .ToList();

                CheckRowLengths(columns.Count,block);

                CheckKeyVariables(columns, block.Where(a => a.Type == (TokenType.Key | TokenType.Parameter)));
                CheckEnumVariables(columns, block.Where(a => (a.Type & (TokenType.EnumMember | TokenType.EnumDesc | TokenType.EnumInit)) != 0));
            }

            CheckDuplicateTableNames(spans);
            CheckDuplicateEnumNames(spans);
        }

        private static void CheckNullAsSpaceAs(List<ParsedSpan> spans)
        {
            ParsedSpan nullAs = spans.FirstOrDefault(a => a.Type == (TokenType.NullAs | TokenType.Parameter));

            if (nullAs == null)
                return;

            ParsedSpan spaceAs = spans.FirstOrDefault(a => a.Type == (TokenType.SpaceAs | TokenType.Parameter));

            if (spaceAs == null)
                return;

            if (nullAs.Text.Equals(spaceAs.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                spaceAs.Errors.Add(new Error(Errors.NullAsSpaceAs, false));
            }
        }

        private static void CheckDuplicateEnumNames(IEnumerable<ParsedSpan> spans)
        {
            IEnumerable<ParsedSpan> duplicateEnumNames = spans
                .Where(a => a.Type == (TokenType.Enum | TokenType.Parameter))
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1));

            foreach (ParsedSpan duplicateEnumName in duplicateEnumNames)
            {
                duplicateEnumName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed, "Enum", duplicateEnumName.Text), false));
            }
        }

        private static void CheckDuplicateTableNames(IEnumerable<ParsedSpan> spans)
        {
            IEnumerable<ParsedSpan> duplicateTableNames = spans
                .Where(a => a.Type == (TokenType.Table | TokenType.Parameter))
                .GroupBy(a => a.Text.ToLower())
                .SelectMany(a => a.Skip(1));

            foreach (ParsedSpan duplicateTableName in duplicateTableNames)
            {
                duplicateTableName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed, "Table", duplicateTableName.Text), false));
            }
        }

        internal static void CheckRowLengths(int headerCount, List<ParsedSpan> spans)
        {
            IOrderedEnumerable<ParsedSpan> itemRows = spans
                .Where(a => a.Type == TokenType.HeaderRow || a.Type == TokenType.ItemRow)
                .OrderBy(a => a.LineNumber);

            foreach (ParsedSpan itemRow in itemRows.Skip(1).Where(a => !a.Errors.Any()))
            {
                var rowCount = itemRow.Text.GetColumns().Count();
                
                if (rowCount != headerCount)
                    itemRow.Errors.Add(new Error(Errors.RowCountMismatch, false));
            }
        }

        internal static void CheckKeyVariables(List<string> columns, IEnumerable<ParsedSpan> keyVars)
        {
            if (keyVars == null)
                return;

            foreach (ParsedSpan key in keyVars)
            {
                string keyValue = key.Text.ToLower();

                if (keyValue.EndsWith("*"))
                    keyValue = $"{keyValue}*";

                if (columns.Contains($"{keyValue}*"))
                {
                    key.Errors.Add(new Error(string.Format(Errors.KeyMapsToNonDBColumn, key.Text), false));
                    continue;
                }

                if (columns.Contains(keyValue))
                    continue;

                key.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns, "Key", key.Text), false));
            }
        }

        internal static void CheckEnumVariables(List<string> columns, IEnumerable<ParsedSpan> enumVars)
        {
            if (enumVars == null)
                return;

            foreach (ParsedSpan enumVar in enumVars)
            {
                string enumValue = enumVar.Text.ToLower();

                if (columns.Select(a => a.TrimEnd('*')).Contains(enumValue))
                    continue;

                enumVar.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns, "Template", enumVar.Text), false));
            }
        }
    }
}
