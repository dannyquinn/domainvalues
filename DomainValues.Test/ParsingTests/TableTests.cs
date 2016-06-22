using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using DomainValues.Processing.Parsing;
using DomainValues.Util;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class TableTests
    {
        [Test]
        public void TableIsRecognised()
        {
            var output = new TableParser().ParseLine(0, "  table", TokenType.Table).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Table, 2, "table", string.Format(Errors.ExpectsParam,"Table"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void TableWithSpacesIsRecognised()
        {
            var output = new TableParser().ParseLine(0," table  ",TokenType.Table).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.Table, 1,"table",string.Format(Errors.ExpectsParam,"Table"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void TableWithParamIsRecognised()
        {
            var output = new TableParser().ParseLine(0, " table  dbo.test  ", TokenType.Table).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Table, 1, "table"),
                new ParsedSpan(0, TokenType.Table | TokenType.Parameter,8, "dbo.test")
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void WordStartingWithTableIsNotRecognised()
        {
            var output = new TableParser().ParseLine(0,"  tabletest ",TokenType.Table).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.Parameter,2,"tabletest", Errors.Invalid);

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void NextTokenShouldBeKey()
        {
            var parser = new TableParser();

            var output =  parser.ParseLine(0, "table", TokenType.Table).Single();

            Assert.AreEqual(TokenType.Key,parser.NextTokenType);
        }

        [Test]
        public void TableParserPrimaryType()
        {
            var parser = new TableParser();

            Assert.AreEqual(TokenType.Table, parser.PrimaryType);
        }

      
    }
}
