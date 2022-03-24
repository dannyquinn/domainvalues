#if !DvLegacy

using DomainValues.Shared.Common;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
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
            var (start, end) = args.TextView.GetSelectionLineBounds();

            using (var edit = args.SubjectBuffer.CreateEdit())
            {
                while (start <= end)
                {
                    var line = edit.Snapshot.GetLineFromLineNumber(start++);

                    var text = line.GetText();

                    if (text.TrimStart().StartsWith("#"))
                    {
                        var index = text.IndexOf("#");

                        var span = new Span(line.Start + index, 1);

                        edit.Replace(span, string.Empty);
                    }
                }

                if (edit.HasEffectiveChanges)
                {
                    edit.Apply();
                }
            }

            return true;
        }

        public CommandState GetCommandState(UncommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
}

#endif