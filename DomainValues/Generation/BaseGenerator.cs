using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace DomainValues.Generation
{
    [ComVisible(true)]
    public abstract class BaseGenerator : IVsSingleFileGenerator
    {
        int IVsSingleFileGenerator.DefaultExtension(out string pbstrDefaultExtension)
        {
            try
            {
                pbstrDefaultExtension = GetDefaultExtension();
                return VSConstants.S_OK;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                pbstrDefaultExtension = string.Empty;
                return VSConstants.E_FAIL;
            }
        }

        int IVsSingleFileGenerator.Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace, IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
            {
                throw new ArgumentException(nameof(bstrInputFileContents));
            }
            InputFilePath = wszInputFilePath;
            FileNamespace = wszDefaultNamespace;
            CodeGeneratorProgress = pGenerateProgress;

            var bytes = GenerateCode(bstrInputFileContents);

            if (bytes == null)
            {
                rgbOutputFileContents = null;
                pcbOutput = 0;
                return VSConstants.E_FAIL;
            }

            var outputLength = bytes.Length;
            rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
            Marshal.Copy(bytes, 0, rgbOutputFileContents[0], outputLength);
            pcbOutput = (uint)outputLength;
            return VSConstants.S_OK;
        }

        protected abstract string GetDefaultExtension();
        protected abstract byte[] GenerateCode(string inputFileContent);

        protected string FileNamespace { get; private set; }

        protected string InputFilePath { get; private set; }

        protected IVsGeneratorProgress CodeGeneratorProgress { get; private set; }

        protected virtual void GeneratorError(uint level, string message, uint line, uint column)
        {
            CodeGeneratorProgress?.GeneratorError(0, level, message, line, column);
        }
        protected virtual void GenerateWaring(uint level, string message, uint line, uint column)
        {
            CodeGeneratorProgress?.GeneratorError(1, level, message, line, column);
        }
    }
}
