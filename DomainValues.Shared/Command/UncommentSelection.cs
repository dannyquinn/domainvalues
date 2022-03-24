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
    [Name(nameof(UncommentSelection))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class UncommentSelection : ICommandHandler<UncommentSelectionCommandArgs>
    {
        public string DisplayName => nameof(UncommentSelection);

        public bool ExecuteCommand(UncommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            args.TextView.UncommentSelection();

            return true;
        }

        public CommandState GetCommandState(UncommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

#endif