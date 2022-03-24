using System;
using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;

namespace DomainValues.Shared.Processing
{
    internal static class Validate
    {
        internal static void CheckBlocks(List<ParsedSpan> spans)
        {
            if (!spans.Any())
            {
                return;
            }

            CheckNullAsSpaceAs(spans);

            foreach (var block in spans.GetStatementBlocks())
            {
                var header = block.FirstOrDefault(a => a.Type == TokenType.HeaderRow);

                if (header == null)
                {
                    continue;
                }

                var columns = header.Text.GetColumns()
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
            var nullAs = spans.FirstOrDefault(a => a.Type == (TokenType.NullAs | TokenType.Parameter));

            if (nullAs == null)
            {
                return;
            }

            var spaceAs = spans.FirstOrDefault(a => a.Type == (TokenType.SpaceAs | TokenType.Parameter));

            if (spaceAs == null)
            {
                return;
            }

            if (nullAs.Text.Equals(spaceAs.Text, StringComparison.CurrentCultureIgnoreCase))
            {
                spaceAs.Errors.Add(new Error(Errors.NullAsSpaceAs, false));
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
                duplicateEnumName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed, "Enum", duplicateEnumName.Text), false));
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
                duplicateTableName.Errors.Add(new Error(string.Format(Errors.NameAlreadyUsed, "Table", duplicateTableName.Text), false));
            }
        }

        internal static void CheckRowLengths(int headerCount, List<ParsedSpan> spans)
        {
            var itemRows = spans
                .Where(a => a.Type == TokenType.HeaderRow || a.Type == TokenType.ItemRow)
                .OrderBy(a => a.LineNumber);

            foreach (var itemRow in itemRows.Skip(1).Where(a => !a.Errors.Any()))
            {
                var rowCount = itemRow.Text.GetColumns().Count();

                if (rowCount != headerCount)
                {
                    itemRow.Errors.Add(new Error(Errors.RowCountMismatch, false));
                }
            }
        }

        internal static void CheckKeyVariables(List<string> columns, IEnumerable<ParsedSpan> keyVars)
        {
            if (keyVars == null)
            {
                return;
            }

            foreach (var key in keyVars)
            {
                var keyValue = key.Text.ToLower();

                if (keyValue.EndsWith("*"))
                {
                    keyValue = $"{keyValue}*";
                }

                if (columns.Contains($"{keyValue}*"))
                {
                    key.Errors.Add(new Error(string.Format(Errors.KeyMapsToNonDBColumn, key.Text), false));
                    continue;
                }

                if (columns.Contains(keyValue))
                {
                    continue;
                }

                key.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns, "Key", key.Text), false));
            }
        }

        internal static void CheckEnumVariables(List<string> columns, IEnumerable<ParsedSpan> enumVars)
        {
            if (enumVars == null)
            {
                return;
            }

            foreach (var enumVar in enumVars)
            {
                var enumValue = enumVar.Text.ToLower();

                if (columns.Select(a => a.TrimEnd('*')).Contains(enumValue))
                {
                    continue;
                }

                enumVar.Errors.Add(new Error(string.Format(Errors.NotFoundInColumns, "Template", enumVar.Text), false));
            }
        }
    }
}
