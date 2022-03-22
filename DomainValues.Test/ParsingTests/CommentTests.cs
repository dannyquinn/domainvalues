using System.Linq;
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
            ParsedSpan output = new CommentParser().ParseLine(0, "  #comment", TokenType.Table).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Comment, 2, "#comment");

            AreEqual(expectedOutput,output);            
        }

        [Test]
        public void NextTokenIsSameAsPassedIn()
        {
            CommentParser parser = new CommentParser();

            ParsedSpan output = parser.ParseLine(0, "#", TokenType.Data).Single();

            Assert.AreEqual(parser.NextExpectedToken, TokenType.Data);

            output = parser.ParseLine(0, "#", TokenType.Table).Single();

            Assert.AreEqual(parser.NextExpectedToken, TokenType.Table);
        }
    }
}
