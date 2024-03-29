﻿#if !DV_LEGACY

using DomainValues.Shared.Common;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DomainValues.Shared.Command
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommentSelection))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class CommentSelection : ICommandHandler<CommentSelectionCommandArgs>
    {
        public string DisplayName => nameof(CommentSelection);

        public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            args.TextView.CommentSelection();
            
            return true;
        }

        public CommandState GetCommandState(CommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

#endif