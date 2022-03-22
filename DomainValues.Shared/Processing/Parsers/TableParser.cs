using DomainValues.Shared.Model;
using System.Collections.Generic;


namespace DomainValues.Shared.Processing.Parsing
{
    internal class TableParser : ParserBase
    {
        protected override TokenType PrimaryType => TokenType.Table;
        protected override TokenType? NextType { get; set; } = TokenType.Key;
        protected override int KeywordLength => 5;
    }
}