using System.Linq;
using DomainValues.Model;
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

            Assert.AreEqual("Key value <id> not found in the column row.", error.Message);
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

            Assert.AreEqual("Key value <id> is marked as non db in the column row.  Cannot be used as a key.", error.Message);
            Assert.IsFalse(error.OutputWindowOnly);
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

            Assert.AreEqual("Key value <id*> not found in the column row.", error.Message);
        }

        [Test]
        public void RowsHaveUnequalNumberOfPipes()
        {
            var test = @"
                table dbo.test
                    key <id>
                    data 
                        | id | test |
                        | 1  | test | thing|
                ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Row count doesn't match header.", output.Message);
        }

        [Test]
        public void TableNameIsDuplicated()
        {
            var test = @"
                table dbo.test
                    key <id>
                    data 
                        | id | test |
                        | 1  | test |

                table dbo.test
                    key <id>
                    data 
                        | id | test |
                        | 1  | test |
                ";

            var output = Parser.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Table named dbo.test already used in this file.", output.Message);
        }
    }
}
