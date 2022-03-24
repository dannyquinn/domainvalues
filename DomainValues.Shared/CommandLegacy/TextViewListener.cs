#if DV_LEGACY

using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace DomainValues.Shared.CommandLegacy
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(DvContent.Id)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class TextViewListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService _editorAdaptersFactoryService = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var view = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            view.TextBuffer.Properties.GetOrCreateSingletonProperty(() => view);

            var commandFilter = new CommandFilter(view);

            textViewAdapter.AddCommandFilter(commandFilter, out var next);

            commandFilter.Next = next;

            view.Closed += ViewClosed;
        }

        private void ViewClosed(object sender, EventArgs e)
        {
            var view = (IWpfTextView)sender;

            view.Closed -= ViewClosed;
        }
    }
}

#endif 