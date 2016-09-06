using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Incident
    {
        public string IncidentNum { get; set; }
        public string OutageStartDateTime { get; set; }
        public string OutageEndDateTime { get; set; }
        public string OutageType { get; set; }
        public string OutageScope { get; set; }
        public string ShortDescription { get; set; }
        public string IncidentStatus { get; set; }
        public string CreatedDt { get; set; }
        public string ProblemId { get; set; }
        public string ClosedDt { get; set; }
        public string OutageStartDt { get; set;}
        public string OutageStartTm { get; set; }
        public string ProblemLink { get; set; }
        public string IncidentLink { get; set; }
        public string ProblemNum { get; set; }
        public string cmd_db { get; set; }

        public bool IsOutageCritical
        {
            get
            {
                return (OutageScope == "Critical") ? true : false;
            }
        }

    }
}