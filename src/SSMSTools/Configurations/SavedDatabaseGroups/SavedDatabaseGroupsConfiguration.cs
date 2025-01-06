using System.Collections.Generic;
using System.Linq;

namespace SSMSTools.Configurations.SavedDatabaseGroups
{
    class SavedDatabaseGroupsConfiguration
    {
        public IEnumerable<SavedDatabaseGroup> Global { get; set; } = Enumerable.Empty<SavedDatabaseGroup>();
    }
}
