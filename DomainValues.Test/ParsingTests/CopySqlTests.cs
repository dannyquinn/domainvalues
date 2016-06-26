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
    public class CopySqlTests
    {
        [Test]
        public void CopySqlIsRecognised()
        {
            ParsedSpan output = new CopySqlParser().ParseLine(0, " copy sql to", TokenType.CopySql).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.CopySql,1,"copy sql to",string.Format(Errors.ExpectsParam,"CopySql"));

            AreEqual(expected, output);
        }

        [Test]
        public void CopySqlWithSpacesIsRecognised()
        {
            ParsedSpan output = new CopySqlParser().ParseLine(0,"copy sql to   ",TokenType.CopySql).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.CopySql,0,"copy sql to",string.Format(Errors.ExpectsParam,"CopySql"));

            AreEqual(expected,output);
        }

        [Test]
        public void CopySqlWithParamIsRecognised()
        {
            List<ParsedSpan> output = new CopySqlParser().ParseLine(0,"copy sql to  test",TokenType.CopySql).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0,TokenType.CopySql,0,"copy sql to"),
                new ParsedSpan(0,TokenType.CopySql | TokenType.Parameter,13,"test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void WordStagingWithCopySqlIsNotRecognised()
        {
            ParsedSpan output = new CopySqlParser().ParseLine(0,"copy sql totest",TokenType.CopySql).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.Parameter,0, "copy sql totest", Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeTable()
        {
            CopySqlParser parser = new CopySqlParser();

            ParsedSpan output = parser.ParseLine(0, "copy sql to", TokenType.CopySql).Single();

            Assert.AreEqual(TokenType.Table,parser.NextExpectedToken);
        }
    }
}
