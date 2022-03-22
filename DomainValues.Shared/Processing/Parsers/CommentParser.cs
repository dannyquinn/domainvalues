using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System.Collections.Generic;
using System.Linq;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class CommentParser : ParserBase
    {
        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            NextExpectedToken = expectedType;

            return new[] {new ParsedSpan(lineNumber, TokenType.Comment, source.GetTextSpan())}.ToList();
        }

        protected override TokenType PrimaryType => TokenType.Comment;
    }
}
