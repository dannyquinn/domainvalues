using System.Linq;
using System.Text;
using DomainValues.Parsing;
using NUnit.Framework;

namespace DomainValues.Test
{
    [TestFixture]
    public class SqlTests
    {
        [Test]
        public void ValidInputNoErrors()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | testcol |
                    | 1  | value   |";

            var output = GetData(test);

            var expected =
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
            var test = @"table dbo.test
                key id testcol
                data 
                    | id | testcol |
                    | 1  | value   |";

            var output = GetData(test);

            var expected =
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
            var test = @"table dbo.test
                key id
                data 
                    | id | testcol | nondb* |
                    | 1  | value   | thing  |";

            var output = GetData(test);

            var expected =
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

        private string GetData(string source)
        {
            var spans = Parser.GetSpans(source, true);

            if (spans.Any(a => a.Errors.Any()))
                return "Error";

            var content = SpansToContent.Convert(spans);

            return Encoding.UTF8.GetString(content.GetSqlBytes());
        }
    }
}
