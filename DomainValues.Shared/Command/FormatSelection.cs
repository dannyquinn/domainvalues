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
    [Name(nameof(FormatSelection))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class FormatSelection : ICommandHandler<FormatSelectionCommandArgs>
    {
        public string DisplayName => nameof(FormatSelection);

        public bool ExecuteCommand(FormatSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            var (start, end) = args.TextView.GetSelectionLineBounds();

            args.TextView.Format(start, end);

            return true;
        }

        public CommandState GetCommandState(FormatSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

#endif