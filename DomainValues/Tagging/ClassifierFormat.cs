using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues.Tagging
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DvContent.DvKeyword)]
    [Name(DvContent.DvKeyword)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DvKeywordFormatDefinition : ClassificationFormatDefinition
    {
        public DvKeywordFormatDefinition()
        {
            DisplayName = "Domain Values Keyword";
            ForegroundColor = ClassifierColor.GetColor(DvContent.DvKeyword);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DvContent.DvHeaderRow)]
    [Name(DvContent.DvHeaderRow)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DvHeaderRowFormatDefinition : ClassificationFormatDefinition
    {
        public DvHeaderRowFormatDefinition()
        {
            DisplayName = "Domain Values Header Row";
            ForegroundColor = ClassifierColor.GetColor(DvContent.DvHeaderRow);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DvContent.DvVariable)]
    [Name(DvContent.DvVariable)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DvVariableFormatDefinition : ClassificationFormatDefinition
    {
        public DvVariableFormatDefinition()
        {
            DisplayName = "Domain Values Variable";
            ForegroundColor = ClassifierColor.GetColor(DvContent.DvVariable);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DvContent.DvText)]
    [Name(DvContent.DvText)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DvTextFormatDefinition : ClassificationFormatDefinition
    {
        public DvTextFormatDefinition()
        {
            DisplayName = "Domain Values Text";
            ForegroundColor = ClassifierColor.GetColor(DvContent.DvText);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = DvContent.DvComment)]
    [Name(DvContent.DvComment)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DvCommentFormatDefinition : ClassificationFormatDefinition
    {
        public DvCommentFormatDefinition()
        {
            DisplayName = "Domain Values Comment";
            ForegroundColor = ClassifierColor.GetColor(DvContent.DvComment);
        }
    }
}
