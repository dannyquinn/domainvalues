using DomainValues.Shared.Model;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class SpaceAsParser : ParserBase
    {
        protected override TokenType PrimaryType => TokenType.SpaceAs;
        protected override TokenType? NextType { get; set; } = TokenType.Table | TokenType.CopySql;
        protected override int KeywordLength => 8;
    }
}
