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
    public class EnumTests
    {
        [Test]
        public void EnumIsRecognised()
        {
            var output = new EnumParser().ParseLine(0, "enum", TokenType.Enum).Single();

            var expected = new ParsedSpan(0, TokenType.Enum, 0, "enum", string.Format(Errors.ExpectsParam,"Enum"));

            AreEqual(expected, output);
        }

        [Test]
        public void EnumWithSpaceIsRecognised()
        {
            var output = new EnumParser().ParseLine(0, " enum  ", TokenType.Enum).Single();

            var expected = new ParsedSpan(0, TokenType.Enum, 1, "enum",string.Format(Errors.ExpectsParam,"Enum"));

            AreEqual(expected, output);
        }

        [Test]
        public void EnumWithTabIsRecognised()
        {
            var output = new EnumParser().ParseLine(0, " enum\t ", TokenType.Enum).Single();

            var expected = new ParsedSpan(0, TokenType.Enum, 1, "enum", string.Format(Errors.ExpectsParam, "Enum"));

            AreEqual(expected, output);
        }

        [Test]
        public void EnumWithParamIsRecognised()
        {
            var output = new EnumParser().ParseLine(0, " enum  test", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 1, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 7, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumWithParamSeparatedByTabIsRecognised()
        {
            var output = new EnumParser().ParseLine(0, " enum\ttest", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 1, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 6, "test")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void WordStartingWithEnumIsNotRecognised()
        {
            var output = new EnumParser().ParseLine(0, " enumtest", TokenType.Enum).Single();

            var expected = new ParsedSpan(0, TokenType.Parameter, 1, "enumtest",Errors.Invalid);

            AreEqual(expected, output);
        }

        [Test]
        public void NextTokenShouldBeTemplate()
        {
            var parser = new EnumParser();

            parser.ParseLine(0, "enum", TokenType.Enum).Single();

            Assert.AreEqual(TokenType.Template, parser.NextExpectedToken);
        }

        [Test]
        public void EnumRecognisesKeywords()
        {
            var output = new EnumParser().ParseLine(0, "enum test internal byte flags", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>()
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 5, "test"),
                new ParsedSpan(0, TokenType.AccessType, 10, "internal"),
                new ParsedSpan(0, TokenType.BaseType, 19, "byte"),
                new ParsedSpan(0, TokenType.FlagsAttribute, 24, "flags")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumRecognisedParamIsMissingWhenKeywordsUsed()
        {
            var output = new EnumParser().ParseLine(0, "enum internal", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum", Errors.EnumNoName),
                new ParsedSpan(0, TokenType.AccessType, 5, "internal")
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumDuplicateAccessType()
        {
            var output = new EnumParser().ParseLine(0, "enum test internal public", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 5, "test"),
                new ParsedSpan(0, TokenType.AccessType, 10, "internal"),
                new ParsedSpan(0, TokenType.AccessType, 19, "public", string.Format(Errors.EnumDuplicate,"AccessType"))
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumDuplicateBaseType()
        {
            var output = new EnumParser().ParseLine(0, "enum test byte int", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 5, "test"),
                new ParsedSpan(0, TokenType.BaseType, 10, "byte"),
                new ParsedSpan(0, TokenType.BaseType, 15, "int",  string.Format(Errors.EnumDuplicate,"BaseType"))
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumDuplicateFlags()
        {
            var output = new EnumParser().ParseLine(0, "enum test flags flags", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 5, "test"),
                new ParsedSpan(0, TokenType.FlagsAttribute, 10, "flags"),
                new ParsedSpan(0, TokenType.FlagsAttribute, 16, "flags",string.Format(Errors.EnumDuplicate,"FlagsAttribute"))
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumDuplicateName()
        {
            var output = new EnumParser().ParseLine(0, "enum test internal test2", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 5, "test"),
                new ParsedSpan(0, TokenType.AccessType, 10, "internal"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 19, "test2", Errors.Invalid)
            };

            AreEqual(expected, output);
        }

        [Test]
        public void EnumOrderIsNotImportant()
        {
            var output = new EnumParser().ParseLine(0, "enum internal byte test flags", TokenType.Enum).ToList();

            var expected = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Enum, 0, "enum"),
                new ParsedSpan(0, TokenType.AccessType, 5, "internal"),
                new ParsedSpan(0, TokenType.BaseType, 14, "byte"),
                new ParsedSpan(0, TokenType.Enum | TokenType.Parameter, 19, "test"),
                new ParsedSpan(0, TokenType.FlagsAttribute, 24, "flags")
            };

            AreEqual(expected, output);
        }
    }
}
