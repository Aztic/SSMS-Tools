using Microsoft.VisualStudio.Shell;
using SSMSTools.Services.Interfaces;

namespace SSMSTools.Services
{
    internal class UIService : IUIService
    {
        public void ValidateUIThread()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
        }
    }
}
