using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class ConnectionBrokerServerHealthMap
    {
        public string ReportId { get; set; }
        public string ConnectionBrokerName { get; set; }

        public virtual ConnectionBroker ConnectionBrokerNameNavigation { get; set; }
        public virtual ServerHealthReport Report { get; set; }
    }
}
