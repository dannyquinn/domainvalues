using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing;
using NUnit.Framework;

namespace DomainValues.Test
{
    [TestFixture]
    public class SpansToContentTest
    {
        [Test]
        public void BasicBlock()
        {
            var test = @"
                table dbo.test
                key id
                data 
                    | id | test_col |
                    | 1  | test_val |";

            var output = GetOutput(test).Single();

            var expected = new DataBlock("dbo.test");

            expected.Data.Add(new Column("id",true,true),new List<string> {"1"} );
            expected.Data.Add(new Column("test_col",false,true),new List<string> {"test_val"} );

            AreEqual(expected, output);
        }

        [Test]
        public void BlockWithEnum()
        {
            var test = @"
                table dbo.test
                key id
                enum testEnum internal byte flags 
                template [test_desc] test_col = id
                data 
                    | id | test_col | test_desc* |
                    | 1  | test_val | test_value |";

            var output = GetOutput(test).Single();

            var expected = new DataBlock("dbo.test")
            {
                EnumName = "testEnum",
                IsEnumInternal = true,
                EnumHasFlagsAttribute = true,
                EnumBaseType = "byte",
                EnumMemberField = "test_col",
                EnumInitField = "id",
                EnumDescField="test_desc"
                
            };

            expected.Data.Add(new Column("id", true, true), new List<string> { "1" });
            expected.Data.Add(new Column("test_col", false, true), new List<string> { "test_val" });
            expected.Data.Add(new Column("test_desc",false,false), new List<string> {"test_value"} );
            AreEqual(expected, output);
        }
        [Test]
        public void TwoBlocks()
        {
            var test = @"
                table dbo.test
                key id 
                data 
                    | id | test_col | 
                    | 1  | test_val | 

                table dbo.testTwo 
                key keyid 
                data 
                    | keyid | test   | 
                    | 1     | value1 |
                    | 2     | value2 | 
            ";

            var output = GetOutput(test).ToList();

            var expected = new List<DataBlock>()
            {
                new DataBlock("dbo.test"),
                new DataBlock("dbo.testTwo")
            };

            var first = expected.Single(a => a.Table.Equals("dbo.test"));
            first.Data.Add(new Column("id", true, true), new List<string> {"1"});
            first.Data.Add(new Column("test_col", false, true), new List<string> {"test_val"});

            var second = expected.Single(a => a.Table.Equals("dbo.testTwo"));
            second.Data.Add(new Column("keyid", true, true), new List<string> {"1", "2"});
            second.Data.Add(new Column("test", false, true), new List<string> {"value1","value2"} );

            AreEqual(first, output.Single(a=>a.Table.Equals("dbo.test")));
            AreEqual(second,output.Single(a=>a.Table.Equals("dbo.testTwo")));

    }

        private void AreEqual(DataBlock expected, DataBlock output)
        {
            Assert.AreEqual(expected.Table,output.Table,"Table");
            Assert.AreEqual(expected.EnumName,output.EnumName,"EnumName");
            Assert.AreEqual(expected.EnumBaseType,output.EnumBaseType,"EnumBaseType");
            Assert.AreEqual(expected.IsEnumInternal,output.IsEnumInternal,"IsEnumInternal");
            Assert.AreEqual(expected.EnumHasFlagsAttribute,output.EnumHasFlagsAttribute,"EnumHasFlagsAttribute");
            Assert.AreEqual(expected.EnumDescField,output.EnumDescField,"EnumDescField");
            Assert.AreEqual(expected.EnumInitField,output.EnumInitField,"EnumInitField");
            Assert.AreEqual(expected.EnumMemberField,output.EnumMemberField,"EnumMemberField");

            for (var i = 0; i < expected.Data.Count; i++)
            {
                Assert.AreEqual(expected.Data.Keys.ElementAt(i),output.Data.Keys.ElementAt(i));

                for (var j = 0; j < expected.Data.Values.ElementAt(i).Count; j++)
                {
                    Assert.AreEqual(expected.Data.Values.ElementAt(i)[j],output.Data.Values.ElementAt(i)[j]);
                }
            }
        }


        private List<DataBlock> GetOutput(string source)
        {
            var blocks = new List<DataBlock>();

            var spans = Scanner.GetSpans(source, true);

            Assert.IsFalse(spans.Any(a=>a.Errors.Any()),"spans.Any(a=>a.Errors.Any())");

            foreach (var block in spans.GetStatementBlocks())
            {
                blocks.Add(SpansToContent.GetBlock(block));    
            }

            return blocks;
        }
    }
}
