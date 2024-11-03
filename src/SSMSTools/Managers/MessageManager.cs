using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SSMSTools.Managers.Interfaces;
using System;

namespace SSMSTools.Managers
{
    internal class MessageManager : IMessageManager
    {
        public void ShowMessageBox(IServiceProvider serviceProvider, string title, string message)
        {
            VsShellUtilities.ShowMessageBox(
                serviceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
