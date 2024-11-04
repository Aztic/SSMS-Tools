using SSMSTools.Models;
using System.Collections.Generic;

namespace SSMSTools.Windows.Interfaces
{
    internal interface IMultiDbQueryRunnerWindow: IBaseWindow
    {
        void SetItems(IEnumerable<CheckboxItem> items);
    }
}
