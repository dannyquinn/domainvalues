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
    public class NullAsTests
    {
        [Test]
        public void NullAsIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, "  null as", TokenType.NullAs).Single();

            var expected = new ParsedSpan(0, TokenType.NullAs, 2, "null as", string.Format(Errors.ExpectsParam,"NullAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithSpacesIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, " null as   ", TokenType.NullAs).Single();

            var expected = new ParsedSpan(0,TokenType.NullAs,1,"null as", string.Format(Errors.ExpectsParam, "NullAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithTabIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, " null as\t   ", TokenType.NullAs).Single();

            var expected = new ParsedSpan(0, TokenType.NullAs, 1, "null as", string.Format(Errors.ExpectsParam, "NullAs"));

            AreEqual(expected, output);
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
        public void NullAsWithParamSeparatedByTabIsRecognised()
        {
            var output = new NullAsParser().ParseLine(0, " null as\ttest  ", TokenType.NullAs).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.NullAs, 1, "null as"),
                new ParsedSpan(0, TokenType.NullAs | TokenType.Parameter, 9, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingWIthNullAsIsNotRecognised()
        {
            var output = new NullAsParser().ParseLine(0," null astest",TokenType.NullAs).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,1,"null astest",Errors.Invalid);

            AreEqual(expected, output);
        }

        [Test]
        public void NextTokenShouldBeTableOrSpaceAsOrCopySql()
        {
            var parser = new NullAsParser();

            parser.ParseLine(0, "null as", TokenType.NullAs).Single();

            Assert.AreEqual(TokenType.Table | TokenType.SpaceAs | TokenType.CopySql,parser.NextExpectedToken);
        }
    }
}
