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
        public string CollectionName { get; set; }
        public double CpuUsageAvg { get; set; }
        public double MemoryUsageAvg { get; set; }
        public double CdriveFreeSpaceAvg { get; set; }
        public double DdriveFreeSpaceAvg { get; set; }
        public double SessionsTotalAvg { get; set; }
        public double SessionsActiveAvg { get; set; }
        public double SessionsDiscAvg { get; set; }
        public double SessionsNullAvg { get; set; }
        public int SessionsTotalSum { get; set; }
        public int SessionsActiveSum { get; set; }
        public int SessionsDiscSum { get; set; }
        public int SessionsNullSum { get; set; }

        public virtual ICollection<Collection> Collection { get; set; }
        public virtual ICollection<ConnectionBrokerServerHealthMap> ConnectionBrokerServerHealthMap { get; set; }
    }
}
