using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using VSOLE = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;

namespace DomainValues.Generation
{
    [ComVisible(true)]
    public abstract class BaseGeneratorWithSite : BaseGenerator, VSOLE.IObjectWithSite
    {
        private object _site;
        private CodeDomProvider _codeDomProvider;
        private ServiceProvider _serviceProvider;
        void VSOLE.IObjectWithSite.GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (_site == null)
            {
                throw new COMException("object is not sited",VSConstants.E_FAIL);
            }

            IntPtr pUnknownPointer = Marshal.GetIUnknownForObject(_site);

            IntPtr intPointer = IntPtr.Zero;
            Marshal.QueryInterface(pUnknownPointer, ref riid, out intPointer);

            if (intPointer == IntPtr.Zero)
            {
                throw new COMException("site does not support requested interface", VSConstants.E_NOINTERFACE);
            }
            ppvSite = intPointer;
        }

        void VSOLE.IObjectWithSite.SetSite(object pUnkSite)
        {
            _site = pUnkSite;
            _codeDomProvider = null;
            _serviceProvider = null;
        }
        private ServiceProvider SiteServiceProvider
        {
            get
            {
                if (_serviceProvider != null)
                    return _serviceProvider;

                _serviceProvider = new ServiceProvider(_site as VSOLE.IServiceProvider);
                Debug.Assert(_serviceProvider != null, "Unable to get ServiceProvider from site object");
                return _serviceProvider;
            }
        }
        protected object GetService(Type serviceType)
        {
            return SiteServiceProvider.GetService(serviceType);
        }
        protected virtual CodeDomProvider GetCodeProvider()
        { 
            if (_codeDomProvider == null) 
            { 
                //Query for IVSMDCodeDomProvider/SVSMDCodeDomProvider for this project type 
                IVSMDCodeDomProvider provider = GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider; 
                if (provider != null) 
                { 
                    _codeDomProvider = provider.CodeDomProvider as CodeDomProvider; 
                } 
                else 
                { 
                    //In the case where no language specific CodeDom is available, fall back to C# 
                    _codeDomProvider = CodeDomProvider.CreateProvider("C#"); 
                } 
            } 
            return _codeDomProvider; 
        } 


        protected ProjectItem GetProjectItem()
        {
            object p = GetService(typeof(ProjectItem));
            Debug.Assert(p != null, "Unable to get ProjectItem");
            return (ProjectItem)p;
        }
        protected Project GetProject() => GetProjectItem().ContainingProject;
        protected VSProjectItem GetVsProjectItem() => (VSProjectItem)GetProjectItem().Object;
        protected VSProject GetVsProject() => (VSProject)GetProject().Object;
    }
}
