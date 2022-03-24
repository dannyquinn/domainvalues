#if DV_LEGACY

using DomainValues.Shared.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Runtime.InteropServices;

namespace DomainValues.Shared.CommandLegacy
{
    internal class CommandFilter : IOleCommandTarget
    {
        private readonly IWpfTextView _view; 
        public CommandFilter(IWpfTextView view)
        {
            _view = view;
        }
        public IOleCommandTarget Next { get; set; }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup != VSConstants.VSStd2K)
            {
                return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }

            switch (prgCmds[0].cmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                case (uint)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_ENABLED;
                    return VSConstants.S_OK;
                default:
                    return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.COMMENT_BLOCK)
                {
                    _view.CommentSelection();
                    return VSConstants.S_OK;
                }

                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK)
                {
                    _view.UncommentSelection();
                    return VSConstants.S_OK;
                }
            }

            int hResult = Next.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

            if (!ErrorHandler.Succeeded(hResult))
            {
                return hResult;
            }

            if (pguidCmdGroup != VSConstants.VSStd2K || (VSConstants.VSStd2KCmdID)nCmdID != VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                return hResult;
            }

            var x = GetTypeChar(pvaIn);

            if (GetTypeChar(pvaIn).Equals('|'))
            {
                _view.AlignTable();
            }

            return hResult;
        }

        private char GetTypeChar(IntPtr pvaIn)
        {
            return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
        }
    }
}

#endif