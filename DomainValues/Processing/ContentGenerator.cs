using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DomainValues.Model;

namespace DomainValues.Processing
{
    internal class ContentGenerator
    {
        private readonly List<DataBlock> _blocks;
        private string _nullOption = string.Empty;
        private string _spaceOption = "$space";

        public ContentGenerator()
        {
            _blocks = new List<DataBlock>();
            CopySql = string.Empty;
        }

        public void UpdateNullAs(string nullAs)
        {
            if (nullAs.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                _nullOption = string.Empty;
                _spaceOption = "$space";
            }
            else {
                _nullOption = nullAs;
                
            }
        }

        public void UpdateSpaceAs(string spaceAs)
        {
            if (spaceAs.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                _spaceOption = string.Empty;
                if (_nullOption == string.Empty)
                {
                    _nullOption = "$null";
                }
            }
            else
            { 
                _spaceOption = spaceAs;
            }


        }

        public string CopySql { get; set; }
        public void AddBlock(DataBlock block) => _blocks.Add(block);

        public byte[] GetEnumBytes(CodeDomProvider provider, string fileNamespace)
        {
            if (_blocks.All(a => string.IsNullOrWhiteSpace(a.EnumName)))
                return null;

            CodeCompileUnit code = new CodeCompileUnit();
            code.UserData.Add("AllowLateBound", false);
            code.UserData.Add("RequiresVariableDeclaration", true);

            CodeNamespace codeNamespace = new CodeNamespace();

            if (_blocks.Any(a => a.EnumHasFlagsAttribute))
                codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

            if (_blocks.Any(a => !string.IsNullOrWhiteSpace(a.EnumDescField)))
                codeNamespace.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));

            code.Namespaces.Add(codeNamespace);

            CodeNamespace enumNamespace = new CodeNamespace(fileNamespace);

            foreach (DataBlock block in _blocks.Where(a => !string.IsNullOrWhiteSpace(a.EnumName)))
            {
                CodeTypeDeclaration type = new CodeTypeDeclaration(block.EnumName)
                {
                    IsEnum = true
                };

                if (block.IsEnumInternal)
                    type.TypeAttributes = TypeAttributes.NotPublic;

                type.BaseTypes.Add(block.GetBaseType());

                if (block.EnumHasFlagsAttribute)
                    type.CustomAttributes.Add(new CodeAttributeDeclaration("Flags"));

                KeyValuePair<Column, List<string>> enumMember = block.Data.Single(a => a.Key.Text.Equals(block.EnumMemberField, StringComparison.CurrentCultureIgnoreCase));
                KeyValuePair<Column, List<string>> enumDesc = block.Data.SingleOrDefault(a => a.Key.Text.Equals(block.EnumDescField, StringComparison.CurrentCultureIgnoreCase));
                KeyValuePair<Column, List<string>> enumInit = block.Data.SingleOrDefault(a => a.Key.Text.Equals(block.EnumInitField, StringComparison.CurrentCultureIgnoreCase));

                for (int i = 0; i < block.Data.Values.ElementAt(0).Count; i++)
                {
                    CodeMemberField field = new CodeMemberField(block.EnumName, enumMember.Value.ElementAt(i));

                    if (enumDesc.Value != null)
                    {
                        field.CustomAttributes.Add(
                            new CodeAttributeDeclaration("Description",
                                new CodeAttributeArgument(
                                    new CodePrimitiveExpression(enumDesc.Value.ElementAt(i))
                                    )
                                ));
                    }

                    if (enumInit.Value != null)
                    {
                        try
                        {
                            object value = Convert.ChangeType(enumInit.Value.ElementAt(i), block.GetBaseType());

                            field.InitExpression = new CodePrimitiveExpression(value);
                        }
                        catch
                        {
                            return Encoding.UTF8.GetBytes($"// Error Generating Output.  Failed to convert value {enumInit.Key.Text}({enumInit.Value.ElementAt(i)}) to type {block.EnumBaseType} on enum {block.EnumName}.");
                        }
                    }

                    type.Members.Add(field);
                }

                enumNamespace.Types.Add(type);
            }
            code.Namespaces.Add(enumNamespace);

            CodeGeneratorOptions options = new CodeGeneratorOptions()
            {
                BlankLinesBetweenMembers = false,
                BracingStyle = "C"
            };



            using (StringWriter writer = new StringWriter(new StringBuilder()))
            {
                provider.GenerateCodeFromCompileUnit(code, writer, options);
                writer.Flush();
                return Encoding.UTF8.GetBytes(writer.ToString());
            }
        }

        public byte[] GetSqlBytes()
        {
            if (!_blocks.Any())
                return null;

            StringBuilder sb = new StringBuilder();

            AddHeader(sb);

            foreach (DataBlock block in _blocks)
            {
                string update = block.Data.Keys.Any(a => !a.IsKey)
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

        private static void AddHeader(StringBuilder sb)
        {
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("-- <auto-generated>");
            sb.AppendLine("--     This code was generated by a tool.");
            sb.AppendLine("--");
            sb.AppendLine("--     Changes to this file may cause incorrect behavior and will be lost if");
            sb.AppendLine("--     the code is regenerated.");
            sb.AppendLine("-- </auto-generated>");
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("");
        }
        private static string SqlUpdateColumns(Dictionary<Column, List<string>> data)
        {
            return data.Keys.Where(a => !a.IsKey && a.IsDbColumn)
                .Select(a => $"TARGET.[{a.Text}] = SOURCE.[{a.Text}]")
                .Aggregate((a, b) => $"{a},{NewLineAndSpace(8)}{b}");
        }

        private static string SqlColumns(Dictionary<Column, List<string>> data, bool horizontal = false, string prefix = null)
        {
            IEnumerable<string> cols = data.Keys
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
