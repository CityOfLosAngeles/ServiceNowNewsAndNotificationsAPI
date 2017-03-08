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

        public string KBNewsCategory
        {
            get
            {
                return "f58f35a7dbd9a200e88577e9af96198e"; //News
            }
        }

        public string KBNewsOutagesKnowledgeBase
        {
            get
            {
                return "ba4b2059db98a2003fc6f1fcbf961987"; //Knews and Outages
            }
        }
    }
}