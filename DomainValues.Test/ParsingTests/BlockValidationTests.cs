using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing;
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
                    key id
                    data 
                        | id | test |
                        | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void KeyIsNotInColumnsRow()
        {
            var test = @"
                table dbo.test
                    key id
                    data 
                        | i  | test |
                        | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a=>a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Key","id"), error.Message);
        }

        [Test]
        public void KeyIsMarkedAsNonDbColumn()
        {
            var test = @"
                table dbo.test
                    key id
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.KeyMapsToNonDBColumn,"id"), error.Message);
            Assert.IsFalse(error.OutputWindowOnly);
        }

        [Test]
        public void KeyWithAsteriskDoesNotMatchNonDbColumn()
        {
            var test = @"
                table dbo.test
                    key id*
                    data 
                        | id* | test |
                        | 1   | test |
                ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Key","id*"), error.Message);
        }

        [Test]
        public void EnumDescIsNotInColumnsRow()
        {
            var test = @"
                table dbo.test
                    key id
                    enum test 
                    template [missing] test = id 
                    data 
                        | id | test |
                        | 1  | test |
                ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns,"Template","missing"), error.Message);
        }

        [Test]
        public void EnumMemberIsNotInColumnsRow()
        {
            var test = @"
                table dbo.test
                    key id
                    enum test 
                    template [test] missing = id 
                    data 
                        | id | test |
                        | 1  | test |
                ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns, "Template", "missing"), error.Message);
        }

        [Test]
        public void EnumInitIsNotInColumnsRow()
        {
            var test = @"
                table dbo.test
                    key id
                    enum test 
                    template [test] test = missing
                    data 
                        | id | test |
                        | 1  | test |
                ";

            var output = Scanner.GetSpans(test, true).ToList();

            var error = output.SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NotFoundInColumns, "Template", "missing"), error.Message);
        }

        [Test]
        public void RowsHaveUnequalNumberOfPipes()
        {
            var test = @"
                table dbo.test
                    key id
                    data 
                        | id | test |
                        | 1  | test | thing|
                ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(Errors.RowCountMismatch, output.Message);
        }

        [Test]
        public void TableNameIsDuplicated()
        {
            var test = @"
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

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NameAlreadyUsed,"Table","dbo.Test"), output.Message);
        }



        [Test]
        public void EnumNameIsDuplicated()
        {
            var test = @"
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

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.NameAlreadyUsed,"Enum","Test"), output.Message);
        }
    }
}
