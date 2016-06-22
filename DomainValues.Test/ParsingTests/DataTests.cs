using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using DomainValues.Model;
using DomainValues.Processing.Parsing;
using DomainValues.Util;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;
namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class DataTests
    {
        [Test]
        public void DataIsRecognised()
        {
            var output = new DataParser().ParseLine(0, "data", TokenType.Data).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Data, 0, "data");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DataWithSpacesIsRecognised()
        {
            var output = new DataParser().ParseLine(0, " data ", TokenType.Data).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Data, 1, "data");

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void WordStartingWithDataIsNotRegognised()
        {
            var output = new DataParser().ParseLine(0, " datatest", TokenType.Data).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Parameter, 1, "datatest", Errors.INVALID);
            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DataWithParamCreatesInvalid()
        {
            var output = new DataParser().ParseLine(0, " data   test  ", TokenType.Data).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Data,1, "data"),
                new ParsedSpan(0, TokenType.Parameter,8, "test", "Data keyword does not have a parameter.")
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void NextTokenShouldBeHeaderRow()
        {
            var parser = new DataParser();

            var output = parser.ParseLine(0, "data", TokenType.Data).Single();

            Assert.AreEqual(TokenType.HeaderRow,parser.NextTokenType);
        }

        [Test]
        public void DataParserPrimaryType()
        {
            var parser = new DataParser();

            Assert.AreEqual(TokenType.Data,parser.PrimaryType);
        }
    }
}
