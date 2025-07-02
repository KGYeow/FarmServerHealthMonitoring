using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class ConnectionBroker
    {
        public ConnectionBroker()
        {
            ConnectionBrokerServerHealthMap = new HashSet<ConnectionBrokerServerHealthMap>();
        }

        public string ServerName { get; set; }

        public virtual ICollection<ConnectionBrokerServerHealthMap> ConnectionBrokerServerHealthMap { get; set; }
    }
}
