using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.PlatformUI;

namespace DomainValues.Command
{
    internal sealed class CommandFilter : IOleCommandTarget
    {
        private readonly Formatter _formatter;

        public CommandFilter(Formatter formatter)
        {
            _formatter = formatter;
        }

        public IOleCommandTarget Next { get; set; }
        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup!=VSConstants.VSStd2K)
                return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);

            switch (prgCmds[0].cmdID)
            {
                case (uint)VSConstants.VSStd2KCmdID.COMMENT_BLOCK:
                case (uint)VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK:
                case (uint)VSConstants.VSStd2KCmdID.FORMATDOCUMENT:
                case (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION:
                    prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_ENABLED;
                    return VSConstants.S_OK;
                default:
                    return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
        }
        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K)
            {
                if (nCmdId == (uint) VSConstants.VSStd2KCmdID.COMMENT_BLOCK)
                {
                    _formatter.CommentSelection();
                    return VSConstants.S_OK;
                }

                if (nCmdId == (int) VSConstants.VSStd2KCmdID.UNCOMMENT_BLOCK)
                {
                    _formatter.RemoveCommentSelection();
                    return VSConstants.S_OK;
                }
                if (nCmdId == (int)VSConstants.VSStd2KCmdID.FORMATDOCUMENT)
                {
                    _formatter.FormatDocument();
                    return VSConstants.S_OK;
                }
                if (nCmdId == (int)VSConstants.VSStd2KCmdID.FORMATSELECTION)
                {
                    _formatter.FormatSelection();
                    return VSConstants.S_OK;
                }
            }

            int hResult = Next.Exec(pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);

            if (!ErrorHandler.Succeeded(hResult))
                return hResult;

            if (pguidCmdGroup != VSConstants.VSStd2K || (VSConstants.VSStd2KCmdID)nCmdId != VSConstants.VSStd2KCmdID.TYPECHAR)
                return hResult;

            if (GetTypeChar(pvaIn).Equals('|'))
            {
               _formatter.AlignTable();
            }
            return hResult;
        }
        private char GetTypeChar(IntPtr pvaIn) => (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
    }
}
