using System.Collections.Generic;
using System.Linq;

namespace SSMSTools.Models
{
    public class ConnectedServerInformation
    {
        public string ServerName { get; set; }
        public IEnumerable<CheckboxItem> Databases { get; set; } = Enumerable.Empty<CheckboxItem>();
    }
}
