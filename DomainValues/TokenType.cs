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
        Parameter       = 1 << 5,
        Comment         = 1 << 6,
        Enum            = 1 << 7,
        Template        = 1 << 8,
        AccessType      = 1 << 9,
        BaseType        = 1 << 10,
        FlagsAttribute  = 1 << 11,
        EnumDesc        = 1 << 12,
        EnumMember      = 1 << 13,
        EnumInit        = 1 << 14,
        NullAs          = 1 << 15,
        SpaceAs         = 1 << 16,
        CopySql         = 1 << 17
    }
}
