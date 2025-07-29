using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class CollectionRecord
    {
        public int Id { get; set; }
        public int CollectionId { get; set; }
        public string ServerName { get; set; }
        public string Enabled { get; set; }
        public string CpuUsage { get; set; }
        public string MemoryUsage { get; set; }
        public string CdriveFreeSpace { get; set; }
        public string DdriveFreeSpace { get; set; }
        public string Uptime { get; set; }
        public string PendingReboot { get; set; }
        public string SessionsTotal { get; set; }
        public string SessionsActive { get; set; }
        public string SessionsDisc { get; set; }
        public string SessionsNull { get; set; }

        public virtual Collection Collection { get; set; }
    }
}
