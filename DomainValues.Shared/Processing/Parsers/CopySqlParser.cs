﻿using DomainValues.Shared.Model;

namespace DomainValues.Shared.Processing.Parsing
{
    internal class CopySqlParser: ParserBase
    {
        protected override TokenType PrimaryType => TokenType.CopySql;
        protected override int KeywordLength => 11;
        protected override TokenType? NextType => TokenType.Table;
    }
}
