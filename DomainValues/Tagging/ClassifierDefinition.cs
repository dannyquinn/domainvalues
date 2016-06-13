using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
namespace DomainValues.Tagging
{
    internal class ClassifierDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(DvContent.DvKeyword)]
        internal static ClassificationTypeDefinition DvKeywordTypeDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(DvContent.DvHeaderRow)]
        internal static ClassificationTypeDefinition DvHeaderRowTypeDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(DvContent.DvVariable)]
        internal static ClassificationTypeDefinition DvVariableTypeDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(DvContent.DvText)]
        internal static ClassificationTypeDefinition DvTextTypeDefinition = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(DvContent.DvComment)]
        internal static ClassificationTypeDefinition DvCommentTypeDefinition = null;
    }
}
