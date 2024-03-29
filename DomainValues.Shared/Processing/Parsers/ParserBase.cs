﻿using DomainValues.Shared.Common;
using DomainValues.Shared.Model;
using System.Collections.Generic;

namespace DomainValues.Shared.Processing.Parsing
{
    internal abstract class ParserBase
    {
        public virtual List<ParsedSpan> ParseLine(int lineNumber, string source,TokenType? expectedType)
        {
            var parsedSpans = new List<ParsedSpan>();

            var span = source.GetTextSpan();

            if (span.Length > KeywordLength &&
                span.Text.Substring(KeywordLength, 1) != " " &&
                span.Text.Substring(KeywordLength, 1) != "\t"
                )
            {
                parsedSpans.Add(new ParsedSpan(lineNumber, TokenType.Parameter, span, Errors.Invalid));
                return parsedSpans;
            }

            var tokenSpan = new ParsedSpan(lineNumber,PrimaryType,span.To(KeywordLength));

            parsedSpans.Add(tokenSpan);

            if (expectedType != null)
            {
                CheckKeywordOrder(tokenSpan, expectedType);
            }

            if (HasParams && span.From(KeywordLength).Length==0)
            {
                tokenSpan.Errors.Add(new Error(string.Format(Errors.ExpectsParam,PrimaryType)));
            }
           
            if (span.Length > KeywordLength)
            {
                foreach (ParsedSpan paramToken in GetParamTokens(lineNumber, span.From(KeywordLength)))
                {
                    parsedSpans.Add(paramToken);
                }
            }

            return parsedSpans;
        }

        protected virtual IEnumerable<ParsedSpan> GetParamTokens(int lineNumber, TextSpan span)
        {
            yield return new ParsedSpan(lineNumber,PrimaryType | TokenType.Parameter,span);
        }

        protected void CheckKeywordOrder(ParsedSpan span, TokenType? expectedType)
        {
            if ((expectedType & PrimaryType) != 0)
            {
                NextExpectedToken = NextType;
                return;
            }

            span.Errors.Add(new Error(string.Format(Errors.UnexpectedKeyword,span.Type,expectedType)));
            NextExpectedToken = null;
        }
        protected abstract  TokenType PrimaryType { get; }
        protected virtual TokenType? NextType { get; set; }
        public TokenType? NextExpectedToken { get; protected set; }
        protected virtual int KeywordLength { get; set; }
        protected virtual bool HasParams => true;
    }
}
