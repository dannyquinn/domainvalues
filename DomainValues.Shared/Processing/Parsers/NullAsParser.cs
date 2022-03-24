using DomainValues.Shared.Model;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class NullAsParser : ParserBase
    {
        protected override TokenType PrimaryType => TokenType.NullAs;
        protected override TokenType? NextType { get; set; } = TokenType.SpaceAs | TokenType.Table | TokenType.CopySql;
        protected override int KeywordLength => 7;
    }
}
