using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Problem
    {
        public Problem()
        {
            Incidents = new List<Incident>();
        }
        public string ProblemNum { get; set; }
        public string SysId { get; set; }
        public string OutageStartDateTime { get; set; }
        public string OutageEndDateTime { get; set; }
        public string OutageType { get; set; }
        public string OutageScope { get; set; }
        public string OutageStatus { get; set; }
        public string ShortDescription { get; set; }
        public string CreatedDt { get; set; }
        public string ProblemLink { get; set; }

        public List<Incident> Incidents { get; set; }

    }
}