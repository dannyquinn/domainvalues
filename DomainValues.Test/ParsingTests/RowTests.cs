using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Parsing;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class RowTests
    {
        [Test]
        public void HeaderRowIsRecognised()
        {
            var output = new RowParser().ParseLine(0, "|A|B|C|", TokenType.HeaderRow).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.HeaderRow, 0, "|A|B|C|");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void ItemRowIsRecognised()
        {
            var output = new RowParser().ParseLine(0,"|1|2|3|",TokenType.ItemRow).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.ItemRow,0,"|1|2|3|");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void NextTokenShouldBeItemRow()
        {
            var parser = new RowParser();

            var output =parser.ParseLine(0,"|1|",TokenType.HeaderRow).Single();

            Assert.AreEqual(TokenType.ItemRow,parser.NextTokenType);
        }

        [Test]
        public void NextTokenShouldBeItemRowOrTableOrData()
        {
            var parser = new RowParser();

            var output = parser.ParseLine(0, "|1|", TokenType.ItemRow).Single();

            Assert.AreEqual(TokenType.Table | TokenType.Data|TokenType.ItemRow,parser.NextTokenType);
        }

        [Test]
        public void RowParserPrimaryType()
        {
            var parser = new RowParser();

            Assert.AreEqual(TokenType.HeaderRow | TokenType.ItemRow, parser.PrimaryType);
        }
    }
}
