using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class ServerHealthReport
    {
        public ServerHealthReport()
        {
            Collection = new HashSet<Collection>();
            ConnectionBrokerServerHealthMap = new HashSet<ConnectionBrokerServerHealthMap>();
        }

        public string Id { get; set; }
        public string ReportName { get; set; }
        public DateTime ScriptStartTime { get; set; }
        public DateTime ScriptEndTime { get; set; }

        public virtual ICollection<Collection> Collection { get; set; }
        public virtual ICollection<ConnectionBrokerServerHealthMap> ConnectionBrokerServerHealthMap { get; set; }
    }
}
