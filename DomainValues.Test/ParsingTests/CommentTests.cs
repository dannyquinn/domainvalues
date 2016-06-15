using System.Linq;
using DomainValues.Model;
using NUnit.Framework;
using DomainValues.Parsing;
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
        public void CommentParserPrimaryType()
        {
            var parser = new CommentParser();

            Assert.AreEqual(parser.PrimaryType,TokenType.Comment);
        }

        [Test]
        public void NextTokenIsSameAsPassedIn()
        {
            var parser = new CommentParser();

            var output = parser.ParseLine(0, "#", TokenType.Data).Single();

            Assert.AreEqual(parser.NextTokenType, TokenType.Data);

            output = parser.ParseLine(0, "#", TokenType.Table).Single();

            Assert.AreEqual(parser.NextTokenType, TokenType.Table);
        }
    }
}
