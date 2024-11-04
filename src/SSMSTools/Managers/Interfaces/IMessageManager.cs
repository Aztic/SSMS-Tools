using System;

namespace SSMSTools.Managers.Interfaces
{
    public interface IMessageManager
    {
        void ShowMessageBox(IServiceProvider serviceProvider, string title, string message);
        void ShowSimpleMessageBox(string content);
    }
}
