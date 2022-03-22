using System.Collections.Generic;
using System.Linq;
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
            ParsedSpan output = new KeyParser().ParseLine(0, "key", TokenType.Key).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0,TokenType.Key, 0,"key",string.Format(Errors.ExpectsParam,"Key"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithSpacesIsRecognised()
        {
            ParsedSpan output = new KeyParser().ParseLine(0, " key  ", TokenType.Key).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Key, 1, "key", string.Format(Errors.ExpectsParam, "Key"));

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithTabIsRecognised()
        {
            ParsedSpan output = new KeyParser().ParseLine(0, " key\t  ", TokenType.Key).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0, TokenType.Key, 1, "key", string.Format(Errors.ExpectsParam, "Key"));

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void KeyWithParamIsRecognised()
        {
            List<ParsedSpan> output = new KeyParser().ParseLine(0, " key  id  ", TokenType.Key).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 1, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 6, "id")
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithParamSeparatedByTabIsRecognised()
        {
            List<ParsedSpan> output = new KeyParser().ParseLine(0, " key\tid  ", TokenType.Key).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 1, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Parameter, 5, "id")
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void WordStartingWithKeyIsNotRecognised()
        {
            ParsedSpan output = new KeyParser().ParseLine(0, "keytest", TokenType.Key).Single();

            ParsedSpan expectedOutput = new ParsedSpan(0,TokenType.Parameter, 0,"keytest", Errors.Invalid);

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithTwoVariables()
        {
            List<ParsedSpan> output = new KeyParser().ParseLine(0, "  key id id2", TokenType.Key).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
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
            List<ParsedSpan> output = new KeyParser().ParseLine(0, "key id a id", TokenType.Key).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
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
            List<ParsedSpan> output = new KeyParser().ParseLine(0, "key id a ID", TokenType.Key).ToList();

            List<ParsedSpan> expectedOutput = new List<ParsedSpan>
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
            KeyParser parser = new KeyParser();

            ParsedSpan output = parser.ParseLine(0, "key", TokenType.Key).Single();

            Assert.AreEqual(TokenType.Data | TokenType.Enum, parser.NextExpectedToken);
        }
    }
}
