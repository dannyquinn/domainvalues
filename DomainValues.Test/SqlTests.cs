using System.Collections.Generic;
using System.Linq;
using System.Text;
using DomainValues.Model;
using DomainValues.Processing;
using DomainValues.Util;
using NUnit.Framework;

namespace DomainValues.Test
{
    [TestFixture]
    public class SqlTests
    {
        [Test]
        public void ValidInputNoErrors()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | testcol |
                    | 1  | value   |";

            string output = GetData(test);

            string expected =
                "MERGE dbo.test AS TARGET\r\n" +
                "USING\r\n" +
                "(\r\n" +
                "    SELECT\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    FROM\r\n" +
                "        (VALUES\r\n" +
                "            (N'1', N'value')\r\n" +
                "        ) AS col_data ([id],[testcol])\r\n" +
                ") AS SOURCE\r\n" +
                "    ON (\r\n" +
                "        TARGET.[id] = SOURCE.[id]\r\n" +
                ")\r\n" +
                "WHEN MATCHED THEN\r\n" +
                "    UPDATE SET\r\n" +
                "        TARGET.[testcol] = SOURCE.[testcol]\r\n" +
                "WHEN NOT MATCHED BY TARGET THEN\r\n" +
                "    INSERT\r\n" +
                "    (\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    )\r\n" +
                "    VALUES\r\n" +
                "    (\r\n" +
                "        SOURCE.[id],\r\n" +
                "        SOURCE.[testcol]\r\n" +
                "    )\r\n" +
                "WHEN NOT MATCHED BY SOURCE THEN DELETE;\r\n\r\n";

            Assert.AreEqual(expected,output);
        }

        [Test]
        public void AllDbColumnsAreKeysDontIncludeUpdate()
        {
            string test = @"table dbo.test
                key id testcol
                data 
                    | id | testcol |
                    | 1  | value   |";

            string output = GetData(test);

            string expected =
                "MERGE dbo.test AS TARGET\r\n"+
                "USING\r\n" +
                "(\r\n" +
                "    SELECT\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    FROM\r\n" +
                "        (VALUES\r\n" +
                "            (N'1', N'value')\r\n" +
                "        ) AS col_data ([id],[testcol])\r\n" +
                ") AS SOURCE\r\n" +
                "    ON (\r\n" +
                "        TARGET.[id] = SOURCE.[id]\r\n" +
                "    AND TARGET.[testcol] = SOURCE.[testcol]\r\n" +
                ")\r\n" +
                "WHEN NOT MATCHED BY TARGET THEN\r\n" +
                "    INSERT\r\n" +
                "    (\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    )\r\n" +
                "    VALUES\r\n" +
                "    (\r\n" +
                "        SOURCE.[id],\r\n" +
                "        SOURCE.[testcol]\r\n" +
                "    )\r\n" + 
                "WHEN NOT MATCHED BY SOURCE THEN DELETE;\r\n\r\n";

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void DontIncludeNonDbColumns()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | testcol | nondb* |
                    | 1  | value   | thing  |";

            string output = GetData(test);

            string expected =
                "MERGE dbo.test AS TARGET\r\n" +
                "USING\r\n" +
                "(\r\n" +
                "    SELECT\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    FROM\r\n" +
                "        (VALUES\r\n" +
                "            (N'1', N'value')\r\n" +
                "        ) AS col_data ([id],[testcol])\r\n" +
                ") AS SOURCE\r\n" +
                "    ON (\r\n" +
                "        TARGET.[id] = SOURCE.[id]\r\n" +
                ")\r\n" +
                "WHEN MATCHED THEN\r\n" +
                "    UPDATE SET\r\n" +
                "        TARGET.[testcol] = SOURCE.[testcol]\r\n" +
                "WHEN NOT MATCHED BY TARGET THEN\r\n" +
                "    INSERT\r\n" +
                "    (\r\n" +
                "        [id],\r\n" +
                "        [testcol]\r\n" +
                "    )\r\n" +
                "    VALUES\r\n" +
                "    (\r\n" +
                "        SOURCE.[id],\r\n" +
                "        SOURCE.[testcol]\r\n" +
                "    )\r\n" +
                "WHEN NOT MATCHED BY SOURCE THEN DELETE;\r\n\r\n";

            Assert.AreEqual(expected, output);
        }

        [Test]
        public void DefaultNullSpaceOptions()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | col1 | col2   | col3  | col4 | col5  |
                    | 1  |      | $space | $null |      | data  |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains("(N'1', null, N'', N'$null', null, N'data')"));
        }

        [Test]
        public void SpaceAsDefaultOption()
        {
            string test = @"
                space as default 

                table dbo.test
                key id
                data 
                    | id | col1 | col2   | col3  | col4 | col5  |
                    | 1  |      | $space | $null |      | data  |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains("(N'1', N'', N'$space', null, N'', N'data')"),output);
        }

        [Test]
        public void NullAsAndSpaceAsAreCustomValues()
        {
            string test = @"
                null as $space
                space as $null 

                table dbo.test
                key id
                data 
                    | id | col1 | col2   | col3  | col4 | col5  |
                    | 1  |      | $space | $null |      | data  |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains("(N'1', N'', null, N'', N'', N'data')"), output);
        }

        [Test]
        public void NullAsAndSpaceAsAreSameValue()
        {
            string test = @"
                null as test
                space as test

                table dbo.test
                key id
                data 
                    | id | col1 | col2   | col3  | col4 | col5  |
                    | 1  |      | $space | $null |      | data  |";

            Error error = Scanner.GetSpans(test, true).SelectMany(a=>a.Errors).Single();

            Assert.AreEqual(Errors.NullAsSpaceAs, error.Message);
        }

        [Test]
        public void SingleQuotesAreDoubled()
        {
            string test = @"
                table dbo.test
                key id
                data 
                    | id | col1        |
                    | 1  | TestStrin'g |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains("(N'1', N'TestStrin''g')"),output);
        }

        [Test]
        public void EscapedPipeIsUnescaped()
        {
            string test = @"
                table dbo.test
                key id
                data 
                    | id | col1         |
                    | 1  | TestStrin\|g |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains("(N'1', N'TestStrin|g')"),output);
        }

        [Test]
        public void EscapedEscapedPipeIsNotUnescaped()
        {
            string test = @"
                table dbo.test
                key id
                data 
                    | id | col1           |
                    | 1  | TestStrin\\\|g |";

            string output = GetData(test);

            Assert.IsTrue(output.Contains(@"(N'1', N'TestStrin\|g')"), output);
        }
        private string GetData(string source)
        {
            List<ParsedSpan> spans = Scanner.GetSpans(source, true);

            if (spans.Any(a => a.Errors.Any()))
                return spans.SelectMany(a => a.Errors.Select(b => b.Message)).Aggregate((a, b) => $"{a}\r\n{b}");

            ContentGenerator content = SpansToContent.Convert(spans);

            return Encoding.UTF8.GetString(content.GetSqlBytes());
        }
    }
}
