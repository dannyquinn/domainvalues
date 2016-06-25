using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using DomainValues.Processing;
using DomainValues.Util;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class BlockValidationTests
    {
        [Test]
        public void BlockIsValidNoErrors()
        {
            string test = @"
                table dbo.test
                    key id
                    data 
                        | id | test |
                        | 1  | test |
            ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void KeyIsNotInColumnsRow()
        {
            string test = @"
                table dbo.test
                    key id
                    data 
                        | i  | test |
                        | 1  | test |
            ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a=>a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Key","id"), error.Message);
        }

        [Test]
        public void KeyIsMarkedAsNonDbColumn()
        {
            string test = @"
                table dbo.test
                    key id
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.KeyMapsToNonDBColumn,"id"), error.Message);
            Assert.IsFalse(error.OutputWindowOnly);
        }

        [Test]
        public void KeyWithAsteriskDoesNotMatchNonDbColumn()
        {
            string test = @"
                table dbo.test
                    key id*
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Key","id*"), error.Message);
        }

        [Test]
        public void EnumDescIsNotInColumnsRow()
        {
            string test = @"
                table dbo.test
                    key id
                    enum test 
                    template [missing] test = id 
                    data 
                        | id | test |
                        | 1  | test |
                ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Template","missing"), error.Message);
        }

        [Test]
        public void EnumMemberIsNotInColumnsRow()
        {
            string test = @"
                table dbo.test
                    key id
                    enum test 
                    template [test] missing = id 
                    data 
                        | id | test |
                        | 1  | test |
                ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns, "Template", "missing"), error.Message);
        }

        [Test]
        public void EnumInitIsNotInColumnsRow()
        {
            string test = @"
                table dbo.test
                    key id
                    enum test 
                    template [test] test = missing
                    data 
                        | id | test |
                        | 1  | test |
                ";

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Error error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns, "Template", "missing"), error.Message);
        }

        [Test]
        public void RowsHaveUnequalNumberOfPipes()
        {
            string test = @"
                table dbo.test
                    key id
                    data 
                        | id | test |
                        | 1  | test | thing|
                ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(Errors.RowCountMismatch, output.Message);
        }

        [Test]
        public void TableNameIsDuplicated()
        {
            string test = @"
                table dbo.test
                    key id
                    data 
                        | id | test |
                        | 1  | test |

                table dbo.Test
                    key id
                    data 
                        | id | test |
                        | 1  | test |
                ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NameAlreadyUsed,"Table","dbo.Test"), output.Message);
        }



        [Test]
        public void EnumNameIsDuplicated()
        {
            string test = @"
                table dbo.test
                    key id
                    enum test
                    template id
                    data 
                        | id | test |
                        | 1  | test |

                table dbo.test1
                    key id
                    enum Test
                    template id
                    data 
                        | id | test |
                        | 1  | test |
                ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NameAlreadyUsed,"Enum","Test"), output.Message);
        }
    }
}
