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

            var expectedOutput = new ParsedSpan(0, TokenType.Parameter, 1, "datatest", Errors.Invalid);
            
            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DataWithParamCreatesInvalid()
        {
            var output = new DataParser().ParseLine(0, " data   test  ", TokenType.Data).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Data,1, "data"),
                new ParsedSpan(0, TokenType.Parameter,8, "test", string.Format(Errors.NoParams,"Data"))
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void NextTokenShouldBeHeaderRow()
        {
            var parser = new DataParser();

            parser.ParseLine(0, "data", TokenType.Data).Single();

            Assert.AreEqual(TokenType.HeaderRow,parser.NextExpectedToken);
        }
   }
}
