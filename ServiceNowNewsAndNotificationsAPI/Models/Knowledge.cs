using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Knowledge
    {
        public string KBNum { get; set; }
        public string SysId { get; set; }        
        public string ShortDescription { get; set; }
        public string CreatedDt { get; set; }
        public string KBLink { get; set; }
        public string Description { get; set; }
        public string WorkflowStatus { get; set; }
        public string PublishedDt { get; set; }
        public string ValidToDt { get; set; }
    }
}