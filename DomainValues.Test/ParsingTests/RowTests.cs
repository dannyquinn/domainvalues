using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing.Parsing;
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
        public void HeaderRowDuplicateValues()
        {
            var output = new RowParser().ParseLine(0,"|A|B|C|A|",TokenType.HeaderRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.HeaderRow, 0, "|A|B|C|A|"),
                new ParsedSpan(0, TokenType.HeaderRow, 7, "A",string.Format(Errors.DuplicateValue,"Column","A"))
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void HeaderRowDuplicateValuesIgnoreCase()
        {
            var output = new RowParser().ParseLine(0, "|A|B|C|a|", TokenType.HeaderRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.HeaderRow, 0, "|A|B|C|a|"),
                new ParsedSpan(0, TokenType.HeaderRow, 7, "a", string.Format(Errors.DuplicateValue,"Column","a"))
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void HeaderRowTextOutsideOfPipeInvalid()
        {
            var output = new RowParser().ParseLine(0, "|A|B|C|test", TokenType.HeaderRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.HeaderRow, 0, "|A|B|C|"),
                new ParsedSpan(0, TokenType.Parameter, 7, "test", Errors.Invalid)
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void HeaderRowTextOutsideOfPipeInvalidEscaped()
        {
            var output = new RowParser().ParseLine(0, @"|A|B|C|test\|", TokenType.HeaderRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.HeaderRow, 0, "|A|B|C|"),
                new ParsedSpan(0, TokenType.Parameter, 7, @"test\|", Errors.Invalid)
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void ItemRowTextOutsideOfPipeInvalidEscaped()
        {
            var output = new RowParser().ParseLine(0, @"|A|B|C|test\|", TokenType.ItemRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.ItemRow, 0, "|A|B|C|"),
                new ParsedSpan(0, TokenType.Parameter, 7, @"test\|", Errors.Invalid)
            };

            AreEqual(expectedOutput, output);
        }

        public void ItemRowTextOutsideOfPipeInvalid()
        {
            var output = new RowParser().ParseLine(0, "|A|B|C|test", TokenType.ItemRow).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.ItemRow, 0, "|A|B|C|"),
                new ParsedSpan(0, TokenType.Parameter, 7, "test", Errors.Invalid)
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void RowLineStarting()
        {
            var output = new RowParser().ParseLine(0, "  |", TokenType.HeaderRow).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.HeaderRow,2,"|");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void NextTokenShouldBeItemRow()
        {
            var parser = new RowParser();

            parser.ParseLine(0,"|1|",TokenType.HeaderRow).Single();

            Assert.AreEqual(TokenType.ItemRow,parser.NextExpectedToken);
        }

        [Test]
        public void NextTokenShouldBeItemRowOrTableOrData()
        {
            var parser = new RowParser();

            parser.ParseLine(0, "|1|", TokenType.ItemRow).Single();

            Assert.AreEqual(TokenType.Table | TokenType.Data|TokenType.ItemRow,parser.NextExpectedToken);
        }
    }
}
