using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class SpaceAsParser : ParserBase
    {
        protected override TokenType PrimaryType => TokenType.SpaceAs;
        protected override TokenType? NextType { get; set; } = TokenType.Table | TokenType.CopySql;
        protected override int KeywordLength => 8;
    }
}
