using System.Text.RegularExpressions;

namespace DomainValues.Util
{
    internal static class RegExpr
    {
        internal static Regex LastPipe = new Regex(@"(?<!\\)\|",RegexOptions.Compiled);
        internal static Regex Columns = new Regex(@"(?<=(^|[^\\])\|)[^\|\\]*(?:\\.[^\|\\]*)*(?=\|)", RegexOptions.Compiled);
        //internal static Regex Variable => new Regex(@"(?<=(^|[^\\]))<[^>\\]*(?:\\.[^>\\]*)*\>", RegexOptions.Compiled);
    }
}
