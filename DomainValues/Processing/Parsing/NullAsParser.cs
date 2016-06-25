using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainValues.Model;
using DomainValues.Util;

namespace DomainValues.Processing.Parsing
{
    internal class NullAsParser : ParserBase
    {
        protected override TokenType PrimaryType => TokenType.NullAs;
        protected override TokenType? NextType { get; set; } = TokenType.SpaceAs | TokenType.Table;
        protected override bool HasParams => true;
        protected override int KeywordLength => 7;
    }
}
