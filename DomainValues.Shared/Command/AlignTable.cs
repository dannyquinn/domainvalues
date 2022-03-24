#if !DV_LEGACY

using DomainValues.Shared.Common;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace DomainValues.Shared.Command
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(AlignTable))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class AlignTable : IChainedCommandHandler<TypeCharCommandArgs>
    {
        public string DisplayName => nameof(AlignTable);

        public void ExecuteCommand(TypeCharCommandArgs args, Action nextCommandHandler, CommandExecutionContext executionContext)
        {
            nextCommandHandler();

            if (args.TypedChar == '|')
            {
                args.TextView.AlignTable();
            }
        }

        public CommandState GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextCommandHandler)
        {
            return CommandState.Available;
        }
    }
}


#endif