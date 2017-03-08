using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceNowNewsAndNotificationsAPI.Models
{
    public class Change
    {
        public string ChangeNumber { get; set; }//problem number
        public string ChangeShortDescription { get; set; }//short description*
        public string ChangeApprovalStatus { get; set; }//type*
        public string ChangeState { get; set; }//outage status*
        public string ChangeETA { get; set; } ////Planned end date
        public string ChangeLink { get; set; }//problem link*
        public string ChangeSysId { get; set; }//outage sysid
        public string ChangeOutageStartDT { get; set; }//outage start datetime*
        public string ChangeOutageEndDT { get; set; }//outage end datetime*
    }
}