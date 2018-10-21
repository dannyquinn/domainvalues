using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

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
            return Next.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
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
