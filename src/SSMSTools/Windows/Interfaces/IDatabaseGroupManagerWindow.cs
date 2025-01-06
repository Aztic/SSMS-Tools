using SSMSTools.Models;
using System;
using System.Collections.Generic;

namespace SSMSTools.Windows.Interfaces
{
    internal interface IDatabaseGroupManagerWindow : IBaseWindow
    {
        event EventHandler ContentSaved;
        event EventHandler RefreshDatabaseList;
        void SetDatabaseGroupId(Guid? id);
        void SetDatabaseGroupName(string databaseGroupName);
        void SetDatabases(IEnumerable<CheckboxItem> databases, bool resetSelectedDatabases);
    }
}
