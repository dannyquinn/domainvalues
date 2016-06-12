using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
    }
}
