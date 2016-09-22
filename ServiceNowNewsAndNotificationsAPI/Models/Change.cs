using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Change
    {
        public string ChangeNumber { get; set; }
        public string ChangeShortDescription { get; set; }
        public string ChangeDescription { get; set; }
        public string PlannedChangeStartDate { get; set; }
        public string PlannedChangeEndDate { get; set; }
        public string ChangeApprovalStatus { get; set; }
        public string ChangeState { get; set; }
        public string ChangeRisk { get; set; }
        public string ChangePriority { get; set; }
        public string ChangeImpact { get; set; }
        public string ChangeLink { get; set; }
    }
}