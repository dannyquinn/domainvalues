using System;

namespace DomainValues
{
    [Flags]
    internal enum TokenType
    {
        Table=1,
        Key=2,
        Data=4,
        HeaderRow=8,
        ItemRow=16,
        Variable = 32,
        Parameter = 64,
        Comment = 128
    }
}
