using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class Collection
    {
        public Collection()
        {
            CollectionRecord = new HashSet<CollectionRecord>();
        }

        public int Id { get; set; }
        public string ReportId { get; set; }
        public string Name { get; set; }
        public string CpuUsageAvg { get; set; }
        public string MemoryUsageAvg { get; set; }
        public string CdriveFreeSpaceAvg { get; set; }
        public string DdriveFreeSpaceAvg { get; set; }
        public string SessionsTotalAvg { get; set; }
        public string SessionsActiveAvg { get; set; }
        public string SessionsDiscAvg { get; set; }
        public string SessionsNullAvg { get; set; }
        public string SessionsTotalSum { get; set; }
        public string SessionsActiveSum { get; set; }
        public string SessionsDiscSum { get; set; }
        public string SessionsNullSum { get; set; }

        public virtual ServerHealthReport Report { get; set; }
        public virtual ICollection<CollectionRecord> CollectionRecord { get; set; }
    }
}
