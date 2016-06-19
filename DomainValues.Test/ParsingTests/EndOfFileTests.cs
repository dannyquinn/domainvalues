using System.Linq;
using DomainValues.Processing;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class EndOfFileTests
    {
        [Test]
        public void CompleteBlocksNoError()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key id
                data 
                    | id | data |
                    | 1  | one  |
                    | 2  | two  |
                data 
                    | id | data  |
                    | 3  | three |
            ";

            var output = Scanner.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void FileEndsWithTableThenError()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithKeyThenError()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key id
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithDataThenError()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key <id>
                data 
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();
            Assert.AreEqual("Unexpected end of file.",output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithHeaderThenError()
        {
            var test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key id
                data 
                    | id | data |";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();
            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }
    }
}
