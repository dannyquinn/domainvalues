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
            ParsedSpan output = new NullAsParser().ParseLine(0, "  null as", TokenType.NullAs).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.NullAs, 2, "null as", string.Format(Errors.ExpectsParam,"NullAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithSpacesIsRecognised()
        {
            ParsedSpan output = new NullAsParser().ParseLine(0, " null as   ", TokenType.NullAs).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.NullAs,1,"null as", string.Format(Errors.ExpectsParam, "NullAs"));

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithTabIsRecognised()
        {
            ParsedSpan output = new NullAsParser().ParseLine(0, " null as\t   ", TokenType.NullAs).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.NullAs, 1, "null as", string.Format(Errors.ExpectsParam, "NullAs"));

            AreEqual(expected, output);
        }

        [Test]
        public void NullAsWithParamIsRecognised()
        {
            List<ParsedSpan> output = new NullAsParser().ParseLine(0," null as test  ",TokenType.NullAs).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.NullAs, 1, "null as"),
                new ParsedSpan(0, TokenType.NullAs | TokenType.Parameter, 9, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void NullAsWithParamSeparatedByTabIsRecognised()
        {
            List<ParsedSpan> output = new NullAsParser().ParseLine(0, " null as\ttest  ", TokenType.NullAs).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.NullAs, 1, "null as"),
                new ParsedSpan(0, TokenType.NullAs | TokenType.Parameter, 9, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingWIthNullAsIsNotRecognised()
        {
            ParsedSpan output = new NullAsParser().ParseLine(0," null astest",TokenType.NullAs).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.Parameter,1,"null astest",Errors.Invalid);
        }

        [Test]
        public void NextTokenShouldBeTableOrSpaceAsOrCopySql()
        {
            NullAsParser parser = new NullAsParser();

            ParsedSpan output = parser.ParseLine(0, "null as", TokenType.NullAs).Single();

            Assert.AreEqual(TokenType.Table | TokenType.SpaceAs | TokenType.CopySql,parser.NextExpectedToken);
        }
    }
}
