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
    public class SpaceAsTests
    {
        [Test]
        public void SpaceAsIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0," space as",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"Space As"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithSpacesIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0," space as  ",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"Space As"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithParamIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0," space as test",TokenType.SpaceAs).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.SpaceAs, 1, "space as"),
                new ParsedSpan(0, TokenType.SpaceAs | TokenType.Parameter, 10, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void WordStartingWithSpaceAsIsNotRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0,"space astest",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,0,"space astest",Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeTable()
        {
            var parser = new SpaceAsParser();

            var output = parser.ParseLine(0, "space as", TokenType.SpaceAs).Single();

            Assert.AreEqual(TokenType.Table , parser.NextTokenType);
        }

        [Test]
        public void SpaceAsParserPrimaryType()
        {
            var parser = new SpaceAsParser();

            Assert.AreEqual(TokenType.SpaceAs,parser.PrimaryType);
        }
    }
}
