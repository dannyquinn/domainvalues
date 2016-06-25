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
            ParsedSpan output = new DataParser().ParseLine(0, "data", TokenType.Data).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Data, 0, "data");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DataWithSpacesIsRecognised()
        {
            ParsedSpan output = new DataParser().ParseLine(0, " data ", TokenType.Data).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Data, 1, "data");

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void WordStartingWithDataIsNotRegognised()
        {
            ParsedSpan output = new DataParser().ParseLine(0, " datatest", TokenType.Data).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Parameter, 1, "datatest", Errors.Invalid);
            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DataWithParamCreatesInvalid()
        {
            List<ParsedSpan> output = new DataParser().ParseLine(0, " data   test  ", TokenType.Data).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Data,1, "data"),
                new ParsedSpan(0, TokenType.Parameter,8, "test", string.Format(Errors.NoParams,"Data"))
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void NextTokenShouldBeHeaderRow()
        {
            DataParser parser = new DataParser();

            ParsedSpan output = parser.ParseLine(0, "data", TokenType.Data).Single();

            Assert.AreEqual(TokenType.HeaderRow,parser.NextExpectedToken);
        }
   }
}
