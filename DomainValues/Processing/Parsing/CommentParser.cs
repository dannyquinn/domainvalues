//using System.Collections.Generic;
//using System.Linq;
//using DomainValues.Model;
//using DomainValues.Util;

//namespace DomainValues.Processing.Parsing
//{
//    internal class CommentParser : ParserBase
//    {
//        public override List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
//        {
//            NextExpectedToken = expectedType;

//            return new[] {new ParsedSpan(lineNumber, TokenType.Comment, source.GetTextSpan())}.ToList();
//        }

//        protected override TokenType PrimaryType => TokenType.Comment;
//    }
//}
