using System.Collections.Generic;
using System.Linq;
using DomainValues.Model;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    internal static class Util
    {
        public static void AreEqual(IList<ParsedSpan> expected, IList<ParsedSpan> test)
        {
            Assert.AreEqual(expected.Count(),test.Count(), "Count");

            foreach (var t in test)
            {
                var ex = expected.SingleOrDefault(a => a.Start == t.Start);

                Assert.IsNotNull(ex, $"No token at position {t.Start}");

                Assert.AreEqual(ex.Type, t.Type,"Type");
                Assert.AreEqual(ex.Text, t.Text,"Text");
                Assert.AreEqual(ex.LineNumber,t.LineNumber,"Line Number");

                Assert.AreEqual( ex.Errors.Count,t.Errors.Count,"Error count");
                foreach (var error in t.Errors)
                {
                    var exErr = ex.Errors.SingleOrDefault(a => a.Message.Equals(error.Message));


                    Assert.IsNotNull(exErr, $"No error '{error}'");
                    Assert.AreEqual(exErr.OutputWindowOnly,error.OutputWindowOnly);
                }
            }
        }

        public static void AreEqual(ParsedSpan expected, ParsedSpan test)
        {
            AreEqual(new List<ParsedSpan> {expected},new List<ParsedSpan> {test} );
        }
    }
}
