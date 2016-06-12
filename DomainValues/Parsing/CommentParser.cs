using System.Collections.Generic;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Parsing
{
    internal class CommentParser : LineParser
    {
        internal override IEnumerable<ParsedSpan> ParseLine(int lineNumber, string source, TokenType? expectedTokenType)
        {
            NextTokenType = expectedTokenType;

            yield return new ParsedSpan(lineNumber,TokenType.Comment,source.GetTextSpan());
        }

        internal override TokenType PrimaryType=>TokenType.Comment;
    }
}
