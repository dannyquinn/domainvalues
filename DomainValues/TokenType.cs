using System;

namespace DomainValues
{
    [Flags]
    internal enum TokenType
    {
        Table           = 1 << 0,
        Key             = 1 << 1,
        Data            = 1 << 2,
        HeaderRow       = 1 << 3,
        ItemRow         = 1 << 4,
        Variable        = 1 << 5,
        Parameter       = 1 << 6,
        Comment         = 1 << 7,
        Enum            = 1 << 8,
        Template        = 1 << 9,
        AccessType      = 1 << 10,
        BaseType        = 1 << 11,
        FlagsAttribute  = 1 << 12,
        EnumDesc        = 1 << 13,
        EnumMember      = 1 << 14,
        EnumInit         =1 << 15,
//      NullAs          = 1 << 16,
//      SpaceAs         = 1 << 17,
//      CopySql         = 1 << 18
    }
}
