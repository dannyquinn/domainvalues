using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Processing.Parsing;
using DomainValues.Util;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;
namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class NullAsTests
    {
        [Test]
        public void NullAsIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, "  null as", TokenType.NullAs).Single();

            var expected = new ParsedSpan(0, TokenType.NullAs, 2, "null as", string.Format(Errors.ExpectsParam,"Null As"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithSpacesIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, " null as   ", TokenType.NullAs).Single();

            var expected = new ParsedSpan(0,TokenType.NullAs,1,"null as", string.Format(Errors.ExpectsParam, "Null As"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithParamIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0," null as test  ",TokenType.NullAs).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.NullAs, 1, "null as"),
                new ParsedSpan(0, TokenType.NullAs | TokenType.Parameter, 9, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void WordStartingWIthNullAsIsNotRecognised()
        {
            var output = new NullAsParser().ParseLine(0," null astest",TokenType.NullAs).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,1,"null astest",Errors.Invalid);
        }

        [Test]
        public void NextTokenShouldBeTableOrSpaceAs()
        {
            var parser = new NullAsParser();

            var output = parser.ParseLine(0, "null as", TokenType.NullAs).Single();

            Assert.AreEqual(TokenType.Table | TokenType.SpaceAs,parser.NextTokenType);
        }

        [Test]
        public void NullAsParserPrimaryType()
        {
            var parser = new NullAsParser();

            Assert.AreEqual(TokenType.NullAs,parser.PrimaryType);
        }
    }
}
