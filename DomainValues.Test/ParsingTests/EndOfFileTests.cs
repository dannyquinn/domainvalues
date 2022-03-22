using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class EndOfFileTests
    {
        [Test]
        public void CompleteBlocksNoError()
        {
            string test = @"table dbo.test
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

            List<ParsedSpan> output = Scanner.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void FileEndsWithTableThenError()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithKeyThenError()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key id
            ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithDataThenError()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key <id>
                data 
            ";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();
            Assert.AreEqual("Unexpected end of file.",output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }

        [Test]
        public void FileEndsWithHeaderThenError()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |

                table dbo.test1 
                key id
                data 
                    | id | data |";

            Error output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();
            Assert.AreEqual("Unexpected end of file.", output.Message);
            Assert.IsTrue(output.OutputWindowOnly);
        }
    }
}
