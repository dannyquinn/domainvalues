﻿using System.Collections.Generic;
using System.Linq;
using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using DomainValues.Shared.Processing;
using NUnit.Framework;

namespace DomainValues.Test.ParsingTests
{
    [TestFixture]
    public class FlowTests
    {
        [Test]
        public void FlowIsOkayNoErrors()
        {
            string test = @"table dbo.test
                key id
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).ToList();

            Assert.IsFalse(output.Any(a=>a.Errors.Any()));
        }

        [Test]
        public void TableIsExpected()
        {
            var test = @"
                key id
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.UnexpectedKeyword, "Key", "Table, NullAs, SpaceAs, CopySql"), output.Message);
        }

        [Test]
        public void KeyIsExpected()
        {
            var test = @"table dbo.test
              
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.UnexpectedKeyword, "Data", "Key"), output.Message);
        }

        [Test]
        public void DataIsExpected()
        {
            var test = @"table dbo.test
                key id
              
                    | id | data | 
                    | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.UnexpectedKeyword,"ItemRow","Data, Enum"), output.Message);
        }

        [Test]
        public void KeyIsExpectedButTableDuplicated()
        {
            var test = @"table dbo.test
                table dbo.test1
                data 
                    | id | data | 
                    | 1  | test |
            ";

            var output = Scanner.GetSpans(test, true).SelectMany(a => a.Errors).Single();

            Assert.AreEqual(string.Format(Errors.UnexpectedKeyword, "Table", "Key"), output.Message);

        }
    }
}
