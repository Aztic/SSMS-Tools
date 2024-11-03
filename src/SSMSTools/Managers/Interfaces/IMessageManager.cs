using System;

namespace SSMSTools.Managers.Interfaces
{
    internal interface IMessageManager
    {
        void ShowMessageBox(IServiceProvider serviceProvider, string title, string message);
    }
}
