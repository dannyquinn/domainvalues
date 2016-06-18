using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Parsing;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class FlowTests
    {
        [Test]
        public void FlowIsOkayNoErrors()
        {
            var test = @"table dbo.test
                key <id>
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void TableIsExpected()
        {
            var test = @"
                key <id>
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Key was unexpected.  Expected Table.", output.Message);
        }

        [Test]
        public void KeyIsExpected()
        {
            var test = @"table dbo.test
              
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Data was unexpected.  Expected Key.", output.Message);
        }

        [Test]
        public void DataIsExpected()
        {
            var test = @"table dbo.test
                key <id>
              
                    | id | data | 
                    | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("ItemRow was unexpected.  Expected Data.", output.Message);
        }

        [Test]
        public void KeyIsExpectedButTableDuplicated()
        {
            var test = @"table dbo.test
                table dbo.test1
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Table was unexpected.  Expected Key.", output.Message);

        }
    }
}
