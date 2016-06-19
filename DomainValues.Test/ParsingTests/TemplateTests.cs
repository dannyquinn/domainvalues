using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DomainValues.Model;
using DomainValues.Parsing;
using NUnit.Framework;
using static DomainValues.Test.ParsingTests.Util;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class TemplateTests
    {
        [Test]
        public void TemplateIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template", TokenType.Template).Single();

            var expected = new ParsedSpan(0, TokenType.Template, 0, "template", "Template expects at least one parameter, the dataitem to use as the enum member.");

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithSpaceIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template   ", TokenType.Template).Single();

            var expected = new ParsedSpan(0, TokenType.Template, 0, "template", "Template expects at least one parameter, the dataitem to use as the enum member.");

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithParamIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template test", TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumMember, 9, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void WordStartingTemplateIsNotRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "templatetest", TokenType.Template).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,0,"templatetest","Invalid text in file.");

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeData()
        {
            var parser = new TemplateParser();

            var output = parser.ParseLine(0, "template", TokenType.Template).Single();

            Assert.AreEqual(TokenType.Data,parser.NextTokenType);
        }

        [Test]
        public void TemplateParserPrimaryType()
        {
            var parser = new TemplateParser();

            Assert.AreEqual(TokenType.Template,parser.PrimaryType);
        }

        [Test]
        public void TemplateWithDesc()
        {
            var output = new TemplateParser().ParseLine(0, "template [test] test2",TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumDesc, 10, "test"),
                new ParsedSpan(0, TokenType.EnumMember, 16, "test2")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void TemplateWithInit()
        {
            var output = new TemplateParser().ParseLine(0, "template test = id", TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumMember, 9, "test"),
                new ParsedSpan(0, TokenType.EnumInit, 16, "id")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void TemplateWithDescAndInit()
        {
            var output = new TemplateParser().ParseLine(0, "template [test] test2 = id", TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumDesc, 10, "test"),
                new ParsedSpan(0, TokenType.EnumMember, 16, "test2"),
                new ParsedSpan(0, TokenType.EnumInit, 24, "id")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void TemplatePatternIsNotRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template test = id test2", TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.Parameter, 9, "test = id test2","Cannot determine meaning from string.")
            };

            AreEqual(expected,output);
        }
    }
}
