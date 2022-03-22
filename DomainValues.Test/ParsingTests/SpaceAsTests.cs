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
    public class SpaceAsTests
    {
        [Test]
        public void SpaceAsIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0," space as",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"SpaceAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithSpacesIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0," space as  ",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.SpaceAs,1,"space as", string.Format(Errors.ExpectsParam,"SpaceAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void SpaceAsWithTabIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0, " space as\t  ", TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0, TokenType.SpaceAs, 1, "space as", string.Format(Errors.ExpectsParam, "SpaceAs"));

            AreEqual(expected, output);
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
        public void SpaceAsWithParamSeparatedByTabIsRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0, " space as\ttest", TokenType.SpaceAs).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.SpaceAs, 1, "space as"),
                new ParsedSpan(0, TokenType.SpaceAs | TokenType.Parameter, 10, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingWithSpaceAsIsNotRecognised()
        {
            var output = new SpaceAsParser().ParseLine(0,"space astest",TokenType.SpaceAs).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,0,"space astest",Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeTableOrCopySql()
        {
            var parser = new SpaceAsParser();

            parser.ParseLine(0, "space as", TokenType.SpaceAs).Single();

            Assert.AreEqual(TokenType.Table |TokenType.CopySql, parser.NextExpectedToken);
        }
    }
}
