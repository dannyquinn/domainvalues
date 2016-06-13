using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Parsing
{
    internal static class SpansToContent
    {
        public static ContentGenerator Convert(List<ParsedSpan> spans)
        {
            var content = new ContentGenerator();

            foreach (var block in spans.GetStatementBlocks())
            {
                var tableName = block.Single(a => a.Type == (TokenType.Table | TokenType.Parameter)).Text;

                var dataBlock = new DataBlock(tableName);

                var keyVars = block
                    .Where(a => a.Type == (TokenType.Key | TokenType.Variable))
                    .Select(a => a.Text.Substring(1, a.Text.Length - 2));

                foreach (var column in GetColumns(block.First(a => a.Type == TokenType.HeaderRow).Text,keyVars))
                {
                    dataBlock.Data.Add(column, new List<string>());
                }

                foreach (var item in block.Where(a => a.Type == TokenType.ItemRow))
                {
                    var columns = GetColumns(item.Text).ToList();

                    for (var i = 0; i < columns.Count(); i++)
                    {
                        dataBlock.Data.Values.ElementAt(i).Add(columns[i]);
                    }
                }

                content.AddBlock(dataBlock);
            }
            return content;
        }

        private static IEnumerable<Column> GetColumns(string source, IEnumerable<string> keyVars)
        {
            return from header in GetColumns(source)
                   let text = header.TrimEnd(' ', '*')
                   let isKey = keyVars.Any(a => a.Equals(text, StringComparison.CurrentCultureIgnoreCase))
                   let isDbColumn = !header.TrimEnd().EndsWith("*")
                   select new Column(text, isKey, isDbColumn);
        }

        private static IEnumerable<string> GetColumns(string source)
        {
            return RegExpr.Columns.Matches(source).Cast<Match>()
                .Select(a=>a.Value
                    .Trim()
                    .Replace("\\\\\\|", "\\\0")
                    .Replace("\\|", "|")
                    .Replace("\\\0", "\\|")
                );
        }
    }
}
