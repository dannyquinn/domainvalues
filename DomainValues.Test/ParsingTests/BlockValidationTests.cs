using System.CodeDom.Compiler;
using System.Linq;
using DomainValues.Parsing;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class BlockValidationTests
    {
        [Test]
        public void BlockIsValidNoErrors()
        {
            var test = @"
                table dbo.test
                    key <id>
                    data 
                        | id | test |
                        | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void KeyIsNotInColumnsRow()
        {
            var test = @"
                table dbo.test
                    key <id>
                    data 
                        | i  | test |
                        | 1  | test |
            ";

            var output = Parser.GetSpans(test, true).ToList();

            var error = output.SelectMany(a=>a.Errors).Single();

            Assert.AreEqual("Key value <id> not found in the column row.", error);
        }

        [Test]
        public void KeyIsMarkedAsNonDbColumn()
        {
            var test = @"
                table dbo.test
                    key <id>
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            var output = Parser.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Key value <id> is marked as non db in the column row.  Cannot be used as a key.", error);
        }

        [Test]
        public void KeyWithAsteriskDoesNotMatchNonDbColumn()
        {
            var test = @"
                table dbo.test
                    key <id*>
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            var output = Parser.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Key value <id*> not found in the column row.", error);
        }
    }
}
