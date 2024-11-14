using SSMSTools.Models;

namespace SSMSTools.Windows.Interfaces
{
    internal interface IMultiDbQueryRunnerWindow: IBaseWindow
    {
        void SetServerInformation(ConnectedServerInformation serverInformation);
    }
}
