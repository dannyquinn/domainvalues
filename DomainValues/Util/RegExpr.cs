using System.Text.RegularExpressions;

namespace DomainValues.Util
{
    internal static class RegExpr
    {
        internal static Regex Columns = new Regex(@"(?<=(^|[^\\])\|)[^\|\\]*(?:\\.[^\|\\]*)*(?=\|)", RegexOptions.Compiled);
    }
}
