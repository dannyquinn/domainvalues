using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System.Collections.Generic;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class DataParser : ParserBase
    {
        protected override IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            yield return new ParsedSpan(lineNumber,TokenType.Parameter,span,string.Format(Errors.NoParams,"Data"));
        }

        protected override TokenType PrimaryType => TokenType.Data;
        protected override TokenType? NextType { get; set; } = TokenType.HeaderRow;
        protected override bool HasParams => false;
        protected override int KeywordLength => 4;
    }
}