using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class Collection
    {
        public int Id { get; set; }
        public string ReportId { get; set; }
        public string ServerName { get; set; }
        public string Enabled { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double CdriveFreeSpace { get; set; }
        public double DdriveFreeSpace { get; set; }
        public string Uptime { get; set; }
        public string PendingReboot { get; set; }
        public int SessionsTotal { get; set; }
        public int SessionsActive { get; set; }
        public int SessionsDisc { get; set; }
        public int SessionsNull { get; set; }

        public virtual ServerHealthReport Report { get; set; }
    }
}
