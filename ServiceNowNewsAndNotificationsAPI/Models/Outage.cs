using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Outage
    {
        public string OutageNumber { get; set; }
        public string OutageDate { get; set; }
        public string OutageTime { get; set; }
        public string OutageCritical { get; set; }
        public string OutageDescription { get; set; }
        public string OutageType { get; set; }
        public string OutageStatus { get; set; }
        public string OutageLink { get; set; }
    }
}