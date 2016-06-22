﻿using System;
using System.Collections.Generic;
using DomainValues.Processing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text.Editor;

namespace DomainValues.Tagging
{
    internal sealed class Classifier : SimpleTagger<ClassificationTag>,IDisposable
    {
        private readonly ITextBuffer _buffer;
        private readonly IClassificationTypeRegistryService _typeRegistry;
        private readonly IClassificationFormatMapService _formatMap;
        private readonly string[] _classifications;
        internal Classifier(IClassificationTypeRegistryService typeRegistry,IClassificationFormatMapService formatMap,ITextBuffer buffer) : base(buffer)
        {
            _buffer = buffer;
            _typeRegistry = typeRegistry;
            _formatMap = formatMap;

            _buffer.Changed += TextBuffer_Changed;
            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            _classifications = new[]
            {
                DvContent.DvKeyword,
                DvContent.DvComment,
                DvContent.DvHeaderRow,
                DvContent.DvText,
                DvContent.DvVariable
            };

            UpdateTagSpans();
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            var view = _buffer.Properties.GetProperty(typeof(IWpfTextView)) as IWpfTextView;

            var format = _formatMap.GetClassificationFormatMap(view);

            format.BeginBatchUpdate();
            
            foreach (var classification in _classifications)
            {
                var classificationType = _typeRegistry.GetClassificationType(classification);

                var property = format.GetTextProperties(classificationType);

                format.SetTextProperties(classificationType,property.SetForeground(ClassifierColor.GetColor(classification)));
            }

            format.EndBatchUpdate();
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            UpdateTagSpans();
        }

        private void UpdateTagSpans()
        {
            using (Update())
            {
                RemoveTagSpans(trackingTagSpan => true);
                CreateTagSpans(_buffer.CurrentSnapshot);
            }
        }

        private void CreateTagSpans(ITextSnapshot snapshot)
        {
            var lineNumber = 0;
            var line = snapshot.GetLineFromLineNumber(0);

            foreach (var span in Scanner.GetSpans(snapshot.GetText(), false))
            {
                if (span.LineNumber > lineNumber)
                {
                    lineNumber = span.LineNumber;
                    line = snapshot.GetLineFromLineNumber(lineNumber);

                }

                var type = _typeRegistry.GetClassificationType(TokenToClassification[span.Type]);

                CreateTag(line, span.Start, span.Text.Length, type);
            }
        }

        private void CreateTag(ITextSnapshotLine line, int index, int length, IClassificationType type)
        {
            var span = line.Snapshot.CreateTrackingSpan(new Span(line.Start + index, length), SpanTrackingMode.EdgeNegative);

            CreateTagSpan(span, new ClassificationTag(type));
        }

        private static readonly Dictionary<TokenType, string> TokenToClassification = new Dictionary<TokenType, string>
        {
            {TokenType.Comment, DvContent.DvComment},

            {TokenType.NullAs, DvContent.DvKeyword},
            {TokenType.NullAs | TokenType.Parameter, DvContent.DvText},

            {TokenType.SpaceAs, DvContent.DvKeyword},
            {TokenType.SpaceAs | TokenType.Parameter, DvContent.DvText},

            {TokenType.Table, DvContent.DvKeyword},
            {TokenType.Table | TokenType.Parameter, DvContent.DvText},

            {TokenType.Key, DvContent.DvKeyword},
            {TokenType.Key | TokenType.Variable, DvContent.DvVariable},

            {TokenType.Enum, DvContent.DvKeyword},
            {TokenType.Enum | TokenType.Parameter, DvContent.DvText},
            {TokenType.AccessType, DvContent.DvKeyword},
            {TokenType.BaseType, DvContent.DvKeyword},
            {TokenType.FlagsAttribute, DvContent.DvKeyword},

            {TokenType.Template, DvContent.DvKeyword},
            {TokenType.EnumDesc, DvContent.DvVariable},
            {TokenType.EnumMember, DvContent.DvVariable},
            {TokenType.EnumInit, DvContent.DvVariable},

            {TokenType.Data, DvContent.DvKeyword},

            {TokenType.HeaderRow, DvContent.DvHeaderRow},

            {TokenType.ItemRow, DvContent.DvText},

            {TokenType.Parameter, DvContent.DvText},
        };

        public void Dispose()
        {
            VSColorTheme.ThemeChanged -= VSColorTheme_ThemeChanged;
            _buffer.Changed -= TextBuffer_Changed;
        }
    }
}
