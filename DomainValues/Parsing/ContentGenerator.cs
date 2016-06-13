using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainValues.Model;

namespace DomainValues.Parsing
{
    internal class ContentGenerator
    {
        private readonly List<DataBlock> _blocks;
        private string _nullOption = string.Empty;
        private string _spaceOption = "$empty";

        public ContentGenerator()
        {
            _blocks = new List<DataBlock>();
        }

        public void AddBlock(DataBlock block) => _blocks.Add(block);

        public byte[] GetEnumBytes()
        {
            throw new NotImplementedException();
        }

        public byte[] GetSqlBytes()
        {
            if (!_blocks.Any())
                return null;

            var sb = new StringBuilder();

            foreach (var block in _blocks)
            {
                var update = block.Data.Keys.Any(a => !a.IsKey)
                    ? string.Format(UpdateTemplate, SqlUpdateColumns(block.Data))
                    : string.Empty;

                sb.AppendFormat(MergeTemplate,
                    block.Table,
                    SqlColumns(block.Data),
                    SqlData(block.Data, _nullOption, _spaceOption),
                    SqlColumns(block.Data, horizontal: true),
                    SqlKeyColumns(block.Data),
                    update,
                    SqlColumns(block.Data, prefix: "SOURCE.")
                    );

                sb.AppendLine();
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string SqlUpdateColumns(Dictionary<Column, List<string>> data)
        {
            return data.Keys.Where(a => !a.IsKey && a.IsDbColumn)
                .Select(a => $"TARGET.[{a.Text}] = SOURCE.[{a.Text}]")
                .Aggregate((a, b) => $"{a},{NewLineAndSpace(8)}{b}");
        }

        private static string SqlColumns(Dictionary<Column, List<string>> data, bool horizontal = false, string prefix = null)
        {
            var cols = data.Keys
                .Where(a => a.IsDbColumn)
                .Select(a => $"{prefix}[{a.Text}]");

            return horizontal
                ? cols.Aggregate((a, b) => $"{a},{b}")
                : cols.Aggregate((a, b) => $"{a},{NewLineAndSpace(8)}{b}");
        }

        private static string SqlKeyColumns(Dictionary<Column, List<string>> data)
        {
            return data.Keys
                .Where(a => a.IsDbColumn && a.IsKey)
                .Select(a => $"TARGET.[{a.Text}] = SOURCE.[{a.Text}]")
                .Aggregate((a, b) => $"{a}{NewLineAndSpace(4)}AND {b}");
        }

        private static string SqlData(Dictionary<Column, List<string>> data, string nullOption, string spaceOption)
        {
            return data
                .Where(a => a.Key.IsDbColumn)
                .Select(a => a.Value)
                .SelectMany(a => a.Select((str, index) => new { str, index }))
                .GroupBy(a => a.index)
                .Select(a => a
                    .Select(b => nullOption.Equals(b.str) ? "null" : spaceOption.Equals(b.str) ? "N''" : $"N'{b.str.Replace("'", "''")}'")
                    .Aggregate((b, c) => $"{b}, {c}")
                )
                .Select(a => $"({a})")
                .Aggregate((a, b) => $"{a},{NewLineAndSpace(12)}{b}");
        }

        private const string MergeTemplate =
            "MERGE {0} AS TARGET\r\n" +
            "USING\r\n" +
            "(\r\n" +
            "    SELECT\r\n" +
            "        {1}\r\n" +
            "    FROM\r\n" +
            "        (VALUES\r\n" +
            "            {2}\r\n" +
            "        ) AS col_data ({3})\r\n" +
            ") AS SOURCE\r\n" +
            "    ON (\r\n" +
            "        {4}\r\n" +
            ")\r\n" +
            "{5}" +
            "WHEN NOT MATCHED BY TARGET THEN\r\n" +
            "    INSERT\r\n" +
            "    (\r\n" +
            "        {1}\r\n" +
            "    )\r\n" +
            "    VALUES\r\n" +
            "    (\r\n" +
            "        {6}\r\n" +
            "    )\r\n" +
            "WHEN NOT MATCHED BY SOURCE THEN DELETE;\r\n";

        private const string UpdateTemplate =
            "WHEN MATCHED THEN\r\n" +
            "    UPDATE SET\r\n" +
            "        {0}\r\n";

        private static readonly Func<int, string> NewLineAndSpace = a => $"\r\n{Space(a)}";

        private static readonly Func<int, string> Space = a => new string(' ', a);
    }
}
