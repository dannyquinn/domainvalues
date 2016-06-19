using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace DomainValues.Command
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType(DvContent.DvContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class VsTextViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        private IVsEditorAdaptersFactoryService _editorAdaptersFactoryService = null;

        [Import]
        private SVsServiceProvider _serviceProvider = null;

        private ErrorListProvider _errorListProvider;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var view = _editorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            view.TextBuffer.Properties.GetOrCreateSingletonProperty(() => view);

            _errorListProvider = view.TextBuffer.Properties.GetOrCreateSingletonProperty(() => new ErrorListProvider(_serviceProvider));

            if (_errorListProvider == null)
                return;

            var filter = new CommandFilter(view);

            IOleCommandTarget next;

            textViewAdapter.AddCommandFilter(filter, out next);

            filter.Next = next;

            view.Closed += View_Closed;
        }

        private void View_Closed(object sender, EventArgs e)
        {
            var view = (IWpfTextView) sender;
            view.Closed -= View_Closed;

            if (_errorListProvider == null)
                return;

            _errorListProvider.Tasks.Clear();
            _errorListProvider.Dispose();
        }
    }
}
