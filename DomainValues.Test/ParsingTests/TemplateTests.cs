using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using DomainValues.Processing.Parsing;
using DomainValues.Util;
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
            ParsedSpan output = new TemplateParser().ParseLine(0, "template", TokenType.Template).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.Template, 0, "template",string.Format(Errors.ExpectsParam,"Template"));

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithSpaceIsRecognised()
        {
            ParsedSpan output = new TemplateParser().ParseLine(0, "template   ", TokenType.Template).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.Template, 0, "template", string.Format(Errors.ExpectsParam,"Template"));

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithTabIsRecognised()
        {
            ParsedSpan output = new TemplateParser().ParseLine(0, "template\t   ", TokenType.Template).Single();

            ParsedSpan expected = new ParsedSpan(0, TokenType.Template, 0, "template", string.Format(Errors.ExpectsParam, "Template"));

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithParamIsRecognised()
        {
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template test", TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumMember, 9, "test")
            };

            AreEqual(expected,output);
        }

        [Test]
        public void TemplateWithParamSeparatedByTabIsRecognised()
        {
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template\ttest", TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumMember, 9, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingTemplateIsNotRecognised()
        {
            ParsedSpan output = new TemplateParser().ParseLine(0, "templatetest", TokenType.Template).Single();

            ParsedSpan expected = new ParsedSpan(0,TokenType.Parameter,0,"templatetest", Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeData()
        {
            TemplateParser parser = new TemplateParser();

            ParsedSpan output = parser.ParseLine(0, "template", TokenType.Template).Single();

            Assert.AreEqual(TokenType.Data,parser.NextExpectedToken);
        }

        [Test]
        public void TemplateWithDesc()
        {
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template [test] test2",TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
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
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template test = id", TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
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
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template [test] test2 = id", TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
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
            List<ParsedSpan> output = new TemplateParser().ParseLine(0, "template test = id test2", TokenType.Template).ToList();

            List<ParsedSpan> expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.Parameter, 9, "test = id test2",Errors.TemplatePatternNotRecognised)
            };

            AreEqual(expected,output);
        }
    }
}
