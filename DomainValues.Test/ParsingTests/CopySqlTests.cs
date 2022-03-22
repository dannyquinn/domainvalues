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
    public class CopySqlTests
    {
        [Test]
        public void CopySqlIsRecognised()
        {
            var output = new CopySqlParser().ParseLine(0, " copy sql to", TokenType.CopySql).Single();

            var expected = new ParsedSpan(0,TokenType.CopySql,1,"copy sql to",string.Format(Errors.ExpectsParam,"CopySql"));

            AreEqual(expected, output);
        }

        [Test]
        public void CopySqlWithSpacesIsRecognised()
        {
            var output = new CopySqlParser().ParseLine(0,"copy sql to   ",TokenType.CopySql).Single();

            var expected = new ParsedSpan(0,TokenType.CopySql,0,"copy sql to",string.Format(Errors.ExpectsParam,"CopySql"));

            AreEqual(expected,output);
        }

        [Test]
        public void CopySqlWithTabIsRecognised()
        {
            var output = new CopySqlParser().ParseLine(0, "copy sql to\t", TokenType.CopySql).Single();

            var expected = new ParsedSpan(0,TokenType.CopySql,0,"copy sql to",string.Format(Errors.ExpectsParam,"CopySql"));

            AreEqual(expected,output);
        }

        [Test]
        public void CopySqlWithParamIsRecognised()
        {
            var output = new CopySqlParser().ParseLine(0,"copy sql to  test",TokenType.CopySql).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0,TokenType.CopySql,0,"copy sql to"),
                new ParsedSpan(0,TokenType.CopySql | TokenType.Parameter,13,"test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void CopySqlWithParamSeparatedByTabIsRecognised()
        {
            var output = new CopySqlParser().ParseLine(0,"copy sql to\ttest",TokenType.CopySql).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0,TokenType.CopySql,0,"copy sql to"),
                new ParsedSpan(0,TokenType.CopySql | TokenType.Parameter,12,"test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStagingWithCopySqlIsNotRecognised()
        {
            var output = new CopySqlParser().ParseLine(0,"copy sql totest",TokenType.CopySql).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,0, "copy sql totest", Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeTable()
        {
            var parser = new CopySqlParser();
            
            parser.ParseLine(0, "copy sql to", TokenType.CopySql).Single();

            Assert.AreEqual(TokenType.Table,parser.NextExpectedToken);
        }
    }
}
