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
    public class TemplateTests
    {
        [Test]
        public void TemplateIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template", TokenType.Template).Single();

            var expected = new ParsedSpan(0, TokenType.Template, 0, "template",string.Format(Errors.ExpectsParam,"Template"));

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithSpaceIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template   ", TokenType.Template).Single();

            var expected = new ParsedSpan(0, TokenType.Template, 0, "template", string.Format(Errors.ExpectsParam,"Template"));

            AreEqual(expected, output);
        }

        [Test]
        public void TemplateWithTabIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template\t   ", TokenType.Template).Single();

            var expected = new ParsedSpan(0, TokenType.Template, 0, "template", string.Format(Errors.ExpectsParam, "Template"));

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
        public void TemplateWithParamSeparatedByTabIsRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "template\ttest", TokenType.Template).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Template, 0, "template"),
                new ParsedSpan(0, TokenType.EnumMember, 9, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingTemplateIsNotRecognised()
        {
            var output = new TemplateParser().ParseLine(0, "templatetest", TokenType.Template).Single();

            var expected = new ParsedSpan(0,TokenType.Parameter,0,"templatetest", Errors.Invalid);

            AreEqual(expected,output);
        }

        [Test]
        public void NextTokenShouldBeData()
        {
            var parser = new TemplateParser();

            parser.ParseLine(0, "template", TokenType.Template).Single();

            Assert.AreEqual(TokenType.Data,parser.NextExpectedToken);
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
                new ParsedSpan(0, TokenType.Parameter, 9, "test = id test2",Errors.TemplatePatternNotRecognised)
            };

            AreEqual(expected,output);
        }
    }
}
