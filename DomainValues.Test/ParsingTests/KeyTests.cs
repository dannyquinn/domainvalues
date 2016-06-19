using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using DomainValues.Parsing;
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

            var expectedOutput = new ParsedSpan(0,TokenType.Key, 0,"key","Key expects at least one parameter.");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithSpacesIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key  ", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0, TokenType.Key, 1, "key", "Key expects at least one parameter.");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithParamIsRecognised()
        {
            var output = new KeyParser().ParseLine(0, " key  id  ", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0, TokenType.Key, 1, "key"),
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 6, "id")
            };

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void WordStartingWithKeyIsNotRecognised()
        {
            var output = new KeyParser().ParseLine(0, "keytest", TokenType.Key).Single();

            var expectedOutput = new ParsedSpan(0,TokenType.Parameter, 0,"keytest","Invalid text in file.");

            AreEqual(expectedOutput,output);
        }

        [Test]
        public void KeyWithTwoVariables()
        {
            var output = new KeyParser().ParseLine(0, "  key id id2", TokenType.Key).ToList();

            var expectedOutput = new List<ParsedSpan>
            {
                new ParsedSpan(0,TokenType.Key,2, "key"),
                new ParsedSpan(0,TokenType.Variable|TokenType.Key,6, "id"),
                new ParsedSpan(0,TokenType.Variable|TokenType.Key,9, "id2")
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
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 4, "id"),
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 7, "a"),
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 9, "id", "Key id is a duplicate value.")
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
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 4, "id"),
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 7, "a"),
                new ParsedSpan(0, TokenType.Key | TokenType.Variable, 9, "ID", "Key ID is a duplicate value.")
            };

            AreEqual(expectedOutput, output);
        }

        [Test]
        public void NextTokenShouldBeData()
        {
            var parser = new KeyParser();

            var output = parser.ParseLine(0, "key", TokenType.Key).Single();

            Assert.AreEqual(TokenType.Data | TokenType.Enum, parser.NextTokenType);
        }

        [Test]
        public void KeyParserPrimaryType()
        {
            var parser = new KeyParser();

            Assert.AreEqual(parser.PrimaryType,TokenType.Key);
        }
    }
}
