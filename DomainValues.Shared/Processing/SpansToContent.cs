﻿using System;
using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;

namespace DomainValues.Shared.Processing
{
    internal static class SpansToContent
    {
        public static ContentGenerator Convert(List<ParsedSpan> spans)
        {
            var content = new ContentGenerator();

            var nullAs = spans.FirstOrDefault(a => a.Type == (TokenType.NullAs | TokenType.Parameter));

            if (nullAs != null)
            {
                content.UpdateNullAs(nullAs.Text);
            }

            var spaceAs = spans.FirstOrDefault(a => a.Type == (TokenType.SpaceAs | TokenType.Parameter));

            if (spaceAs != null)
            {
                content.UpdateSpaceAs(spaceAs.Text);
            }

            var copySql = spans.FirstOrDefault(a => a.Type == (TokenType.CopySql | TokenType.Parameter));

            if (copySql != null)
            {
                content.CopySql = copySql.Text;
            }

            foreach (var block in spans.GetStatementBlocks())
            {
                content.AddBlock(GetBlock(block));
            }
            return content;
        }

        internal static DataBlock GetBlock(List<ParsedSpan> block)
        {
            string tableName = block.Single(a => a.Type == (TokenType.Table | TokenType.Parameter)).Text;

            var dataBlock = new DataBlock(tableName);

            var keyVars = block
                .Where(a => a.Type == (TokenType.Key | TokenType.Parameter))
                .Select(a => a.Text);

            foreach (var column in GetColumns(block.First(a => a.Type == TokenType.HeaderRow).Text, keyVars))
            {
                dataBlock.Data.Add(column, new List<string>());
            }

            foreach (var item in block.Where(a => a.Type == TokenType.ItemRow))
            {
                var columns = item.Text.GetColumns().ToList();

                for (var i = 0; i < columns.Count(); i++)
                {
                    dataBlock.Data.Values.ElementAt(i).Add(columns[i]);
                }
            }

            // enum

            if (block.Any(a => a.Type == TokenType.Enum))
            {
                dataBlock.EnumName = block.Single(a => a.Type == (TokenType.Enum | TokenType.Parameter)).Text;
                dataBlock.EnumBaseType = block.SingleOrDefault(a => a.Type == TokenType.BaseType)?.Text ?? "int";
                dataBlock.IsEnumInternal = block.SingleOrDefault(a => a.Type == TokenType.AccessType)?.Text.ToLower() == "internal";
                dataBlock.EnumHasFlagsAttribute = block.Any(a => a.Type == TokenType.FlagsAttribute);

                dataBlock.EnumDescField = block.SingleOrDefault(a => a.Type == TokenType.EnumDesc)?.Text;
                dataBlock.EnumMemberField = block.Single(a => a.Type == TokenType.EnumMember).Text;
                dataBlock.EnumInitField = block.SingleOrDefault(a => a.Type == TokenType.EnumInit)?.Text;
            }
            return dataBlock;
        }

        private static IEnumerable<Column> GetColumns(string source, IEnumerable<string> keyVars)
        {
            return from header in source.GetColumns()
                   let text = header.TrimEnd(' ', '*')
                   let isKey = keyVars.Any(a => a.Equals(text, StringComparison.CurrentCultureIgnoreCase))
                   let isDbColumn = !header.TrimEnd().EndsWith("*")
                   select new Column(text, isKey, isDbColumn);
        }
    }
}
