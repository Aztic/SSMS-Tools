using System;
using System.Collections.Generic;
using System.Linq;

namespace SSMSTools.Configurations.SavedDatabaseGroups
{
    public class SavedDatabaseGroup
    {
        public Guid? Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> Databases { get; set; } = Enumerable.Empty<string>();
    }
}
