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
    public class KeyTests
    {
        [Test]
        public void KeyIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, "key", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.Key, 0,"key",string.Format(Errors.ExpectsParam,"Key"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithSpacesIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key  ", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Key, 1, "key", string.Format(Errors.ExpectsParam, "Key"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithTabIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key\t  ", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Key, 1, "key", string.Format(Errors.ExpectsParam, "Key"));

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void KeyWithParamIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key  id  ", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 1, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 6, "id")
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithParamSeparatedByTabIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key\tid  ", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 1, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 5, "id")
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void WordStartingWithKeyIsNotRecognised()
        {
            var output = new KeyParser().ParseLine(0, "keytest", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.Parameter, 0,"keytest", Errors.Invalid);

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithTwoVariables()
        {
            var output = new KeyParser().ParseLine(0, "  key id id2", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0,TokenType.Key,2, "key"),
                new ParsedSpan(0,TokenType.Parameter|TokenType.Key,6, "id"),
                new ParsedSpan(0,TokenType.Parameter|TokenType.Key,9, "id2")
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DuplicateVariables()
        {
            var output = new KeyParser().ParseLine(0, "key id a id", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 0, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 4, "id"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 7, "a"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 9, "id", string.Format(Errors.DuplicateValue,"Key","id"))
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void DuplicateVariablesIgnoreCase()
        {
            var output = new KeyParser().ParseLine(0, "key id a ID", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 0, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 4, "id"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 7, "a"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 9, "ID", string.Format(Errors.DuplicateValue,"Key","ID"))
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void NextTokenShouldBeData()
        {
            var parser = new KeyParser();

            parser.ParseLine(0, "key", TokenType.Key).Single();

            Assert.AreEqual(TokenType.Data | TokenType.Enum, parser.NextExpectedToken);
        }
    }
}
