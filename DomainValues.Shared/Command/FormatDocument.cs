#if !DV_LEGACY

using DomainValues.Shared.Common;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace DomainValues.Shared.Command
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(FormatDocument))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class FormatDocument : ICommandHandler<FormatDocumentCommandArgs>
    {
        public string DisplayName => nameof(FormatDocument);

        public bool ExecuteCommand(FormatDocumentCommandArgs args, CommandExecutionContext executionContext)
        {
            args.TextView.Format(0, args.SubjectBuffer.CurrentSnapshot.LineCount - 1);

            return true;
        }

        public CommandState GetCommandState(FormatDocumentCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

#endif