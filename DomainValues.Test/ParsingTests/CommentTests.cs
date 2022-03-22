using System.Linq;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing.Parsing;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void ParseCommentLine()
        {
            var output = new CommentParser().ParseLine(0, "  #comment", TokenType.Table).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Comment, 2, "#comment");

            AreEqual(expectedOutput,output);            
        }

        [Test]
        public void NextTokenIsSameAsPassedIn()
        {
            var parser = new CommentParser();

            parser.ParseLine(0, "#", TokenType.Data).Single();

            Assert.AreEqual(parser.NextExpectedToken, TokenType.Data);

            parser.ParseLine(0, "#", TokenType.Table).Single();

            Assert.AreEqual(parser.NextExpectedToken, TokenType.Table);
        }
    }
}
