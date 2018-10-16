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
            ParsedSpan output = new SpaceAsParser().ParseLine(0," space as",TokenType.SpaceAs).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"SpaceAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithSpacesIsRecognised()
        {
            ParsedSpan output = new SpaceAsParser().ParseLine(0," space as  ",TokenType.SpaceAs).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"SpaceAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithTabIsRecognised()
        {
            ParsedSpan output = new SpaceAsParser().ParseLine(0, " space as\t  ", TokenType.SpaceAs).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.SpaceAs, 1, "space as", string.Format(Errors.ExpectsParam, "SpaceAs"));

            AreEqual(expected, output);
        }

        [Test]
        public void SpaceAsWithParamIsRecognised()
        {
            List<ParsedSpan> output = new SpaceAsParser().ParseLine(0," space as test",TokenType.SpaceAs).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.SpaceAs, 1, "space as"),
                new ParsedSpan(0, TokenType.SpaceAs | TokenType.Parameter, 10, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithParamSeparatedByTabIsRecognised()
        {
            List<ParsedSpan> output = new SpaceAsParser().ParseLine(0, " space as\ttest", TokenType.SpaceAs).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.SpaceAs, 1, "space as"),
                new ParsedSpan(0, TokenType.SpaceAs | TokenType.Parameter, 10, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingWithSpaceAsIsNotRecognised()
        {
            ParsedSpan output = new SpaceAsParser().ParseLine(0,"space astest",TokenType.SpaceAs).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.Parameter,0,"space astest",Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeTableOrCopySql()
        {
            SpaceAsParser parser = new SpaceAsParser();

            ParsedSpan output = parser.ParseLine(0, "space as", TokenType.SpaceAs).Single();

            Assert.AreEqual(TokenType.Table |TokenType.CopySql, parser.NextExpectedToken);
        }
    }
}
