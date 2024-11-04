using SSMSTools.Models;
using System.Collections.Generic;

namespace SSMSTools.Windows.Interfaces
{
    internal interface IMultiDbQueryRunnerWIndow: IBaseWindow
    {
        void SetItems(IEnumerable<CheckboxItem> items);
    }
}
