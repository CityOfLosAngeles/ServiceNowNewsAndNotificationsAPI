using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using ServiceNowNewsAndNotificationsAPI.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Configuration;


namespace ServiceNowNewsAndNotificationsAPI
{
    public class ProcessData
    {
        public List<Problem> DataMassage(string incidentData, string problemData)
        {
            List<Incident> incidentList = new List<Incident>();
            List<Problem> problemList = new List<Problem>();

            // Get Problem data from ServiceNow
            JObject problemsObj = JObject.Parse(problemData);
            JArray problemArray = (JArray)problemsObj["result"];


            // Get Incident data from ServiceNow
            JObject incidentsObj = JObject.Parse(incidentData);
            JArray incidentArray = (JArray)incidentsObj["result"];

            for (var i = 0; i < incidentArray.Count; i++)
            {
                var rec = incidentArray[i];
                string statusValue = rec["state"].ToString();
                //switch (Convert.ToString(rec["state"]))
                //{
                //    case "1":
                //        statusValue = "New";
                //        break;
                //    case "2":
                //        statusValue = "Active";
                //        break;
                //    case "3":
                //        statusValue = "Awaiting Problem";
                //        break;
                //    case "4":
                //        statusValue = "Awaiting User Info";
                //        break;
                //    case "6":
                //        statusValue = "Resolved";
                //        break;
                //    case "7":
                //        statusValue = "Closed";
                //        break;
                //}

                if (FilterReqsFromPoliceDept(rec, "Incident"))
                    continue;

                //check incident closed date if within 24hr then show
                DateTime incClosedDate = new DateTime();
                if (Convert.ToString(rec["closed_at"]) != string.Empty)
                {
                    incClosedDate = Convert.ToDateTime(rec["closed_at"]);
                }

                var dateDiff = (DateTime.Now - incClosedDate).TotalDays;
                if (string.IsNullOrWhiteSpace(rec["closed_at"].ToString()) || dateDiff <= 1)
                {
                    //create new incident holder
                    var inc = new Incident();

                    string incStartDate = "";
                    //Outage information from Incident has been removed
                    //if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                    //{
                    //    incStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("MM/dd/yyyy hh:mm tt");
                    //}

                    //string incEndDate = "";
                    //if (Convert.ToString(rec["u_outage_end_dttm"]) != string.Empty)
                    //{
                    //    incEndDate = Convert.ToDateTime(rec["u_outage_end_dttm"]).ToString("MM/dd/yyyy hh:mm tt");
                    //}

                    string incCreatedDate = "";
                    if (Convert.ToString(rec["opened_at"]) != string.Empty)
                    {
                        incCreatedDate = Convert.ToDateTime(rec["opened_at"]).ToString("MM/dd/yyyy hh:mm tt");
                    }

                    //Outage information from Incident has been removed
                    //string outageStartDate = "";
                    //string outageStartTime = "";
                    //if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                    //{
                    //    outageStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("MM/dd/yyyy");
                    //    outageStartTime = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("hh:mm tt");
                    //}

                    inc.IncidentNum = Convert.ToString(rec["number"]); //incident number
                    inc.IncidentStatus = (statusValue == "Awaiting Problem") ? "Related" : statusValue; //New, Active, Awaiting Problem, Awawiting User Info, Resolved, Closed

                    //inc.OutageStartDateTime = (incStartDate != string.Empty) ? incStartDate : ""; //outage start datetime
                    //inc.OutageEndDateTime = (incEndDate != string.Empty) ? incEndDate : "";//outage end datetime
                    //inc.OutageScope = Convert.ToString(rec["u_outage_scope"]); //outage scope [critical or not]
                    //inc.OutageType = Convert.ToString(rec["u_outage_type"]); //Type of Outage (Planned, Unplanned or Awaiting Problem)

                    inc.ShortDescription = Convert.ToString(rec["short_description"]); //short description
                    inc.CreatedDt = incCreatedDate; //open date
                    //inc.OutageStartDt = outageStartDate;
                    //inc.OutageStartTm = outageStartTime;
                    dynamic problemIdValue = JsonConvert.DeserializeObject(Convert.ToString(rec["problem_id"]));
                    if (problemIdValue != null)
                    {
                        inc.ProblemId = problemIdValue.value; //Convert.ToString(rec["problem_id"]); //problem sysid
                        inc.ProblemLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=problem.do?sys_id=" + inc.ProblemId;
                    }
                    else
                    {
                        inc.ProblemId = "";
                        inc.ProblemLink = "";
                    }
                    inc.ClosedDt = (Convert.ToString(rec["closed_at"]) != string.Empty) ? incClosedDate.ToString("MM/dd/yyyy hh:mm tt") : ""; //closed date
                    inc.IncidentLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=incident.do?sys_id=" + Convert.ToString(rec["sys_id"]);
                    incidentList.Add(inc);
                }
            }
           
            #region not adding unrelated incidents to problems
            //// This sectioncommented out to remove incidents that are unrelated to a problem from the feed.
            //// TODO: Add Incident at top level to problemList
            //// Reset sort order for incidents
            //var incidentSort = 1;
            //var unrelatedIncs = incidentList.Where(w => w.ProblemId == "");
            //if (unrelatedIncs.Count() > 0)
            //{
            //    // Sort the unrelated Incident List by date and time
            //    unrelatedIncs = unrelatedIncs.OrderByDescending(d => d.OutageStartDateTime).ToList();

            //    problemList.Add(new Problem()
            //    {
            //        ProblemNum = "PRBNOPROBLEM",
            //        OutageStartDateTime = "",
            //        OutageEndDateTime = "",
            //        OutageScope = "",
            //        OutageType = "",
            //        OutageStatus = "",
            //        ShortDescription = "",
            //        CreatedDt = "",
            //        SysId = "",
            //        ProblemLink = "",
            //        Incidents = unrelatedIncs.ToList()
            //    });
            //}

            //// Assign unrelated incidents' problem num
            //foreach (var unrelatedinc in unrelatedIncs)
            //{
            //    // Set the Related Incident Sort Order
            //    unrelatedinc.SortOrder = incidentSort.ToString();
            //    incidentSort += 1;

            //    unrelatedinc.ProblemNum = "PRBNOPROBLEM";
            //}
            #endregion

            // Set Problem data with related Incidents
            for (var i = 0; i < problemArray.Count; i++)
            {
                var rec = problemArray[i];
                string prbStartDate = "";
                string prbEndDate = "";
                string prbCreatedDate = "";

                if (FilterReqsFromPoliceDept(rec, "Problem"))
                    continue;

                if (Convert.ToString(rec["u_outage_start_date_time"]) != string.Empty)
                {
                    prbStartDate = Convert.ToDateTime(rec["u_outage_start_date_time"]).ToString("MM/dd/yyyy hh:mm tt");
                }
                else
                {
                    prbStartDate = "";
                }

                if (Convert.ToString(rec["u_outage_end_date_time"]) != string.Empty)
                {
                    prbEndDate = Convert.ToDateTime(rec["u_outage_end_date_time"]).ToString("MM/dd/yyyy hh:mm tt");
                }
                else
                {
                    prbEndDate = "";
                }

                if (Convert.ToString(rec["opened_at"]) != string.Empty)
                {
                    prbCreatedDate = Convert.ToDateTime(rec["opened_at"]).ToString("MM/dd/yyyy hh:mm tt");
                }
                else
                {
                    prbCreatedDate = "";
                }
                if (Convert.ToString(rec["ended_at"]) != string.Empty)
                {

                }
                DateTime probEndDate = new DateTime();
                if (Convert.ToString(rec["u_outage_end_date_time"]) != string.Empty)
                {
                    probEndDate = Convert.ToDateTime(rec["u_outage_end_date_time"]);
                }
                var dateDiff = (prbEndDate != "") ? (DateTime.Now - probEndDate).TotalDays : 10;
                if (prbEndDate == "" || dateDiff <= 1)
                {
                    problemList.Add(new Problem()
                    {
                        ProblemNum = Convert.ToString(rec["number"]), //problem number
                        OutageStartDateTime = prbStartDate, //outage start datetime
                        OutageEndDateTime = (prbEndDate != string.Empty) ? prbEndDate : "",//outage end datetime
                        OutageScope = Convert.ToString(rec["u_outage_scope"]), //outage scope [critical or not]
                        //OutageType = Convert.ToString(rec["u_outage_type"]), //Type of Outage (Planned, Unplanned or Awaiting Problem) Removed
                        OutageStatus = Convert.ToString(rec["u_outage_status"]), //Outage Status (Investigating, In Progress, Resolved, Closed)
                        ShortDescription = Convert.ToString(rec["short_description"]), //short description
                        CreatedDt = prbCreatedDate, //open date
                        SysId = Convert.ToString(rec["sys_id"]), //problem sysid
                        ProblemLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=problem.do?sys_id=" + Convert.ToString(rec["sys_id"])
                    });
                }
            }

            // Sort the Problem List by Outage Status (Investigating, In Progress, Resolved, and Closed)
            problemList = problemList.OrderByDescending(p => p.OutageStatus == "Investigating")
                .ThenByDescending(p => p.OutageStatus == "In progress")
                .ThenByDescending(p => p.OutageStatus == "Resolved")
                .ThenByDescending(p => p.OutageStatus == "Closed")
                .ThenByDescending(p => p.OutageStartDateTime).ToList();


            var problemSort = 1;
            var incidentSort = 1;
            var relatedProblemNum = "";

            //Build Problem and Incident RelationShip
            foreach (var p in problemList)
            {
                // Set Problem Sort Order
                p.ProbSortOrder = problemSort.ToString();
                problemSort += 1;
               
                var relatedIncs = incidentList.Where(x => x.ProblemId == p.SysId);
                if (relatedIncs.Count() > 0)
                {
                    // Sort the related Incident List by date and time
                    relatedIncs = relatedIncs.OrderByDescending(d => d.CreatedDt).ToList();

                    foreach (var inc in relatedIncs)
                    {
                        if (relatedProblemNum != "" && inc.ProblemId != relatedProblemNum)
                        {
                            // Reset the Incident sort order
                            incidentSort = 1;
                            relatedProblemNum = "";
                        }

                        // Set the Related Incident Sort Order
                        if (relatedProblemNum == "" || inc.ProblemId == relatedProblemNum)
                        {
                            inc.IncSortOrder = incidentSort.ToString();
                            incidentSort += 1;
                            relatedProblemNum = inc.ProblemId;
                        }

                        inc.ProblemNum = p.ProblemNum;
                    }
                    p.Incidents.AddRange(relatedIncs);
                }
            }


            if (problemList.Count <= 0)
            {
                List<Incident> emptyList = new List<Incident>();
                problemList.Add(new Problem()
                {
                    //ProblemNum = "PRBNoProblem",
                    ProblemNum = "PRBNONE",
                    OutageStartDateTime = "",
                    OutageEndDateTime = "",
                    OutageScope = "",
                    //OutageType = "",
                    OutageStatus = "",
                    ShortDescription = "No Current Issues at this time.",
                    CreatedDt = "",
                    SysId = "",
                    ProblemLink = "",
                    Incidents = emptyList.ToList()
                });
            }

            return problemList;
        }

        public List<Knowledge> GetNews(string kbData)
        {
            JObject kbObj = JObject.Parse(kbData);
            JArray kbResultSets = (JArray)kbObj["result"];
            List<Knowledge> kbList = new List<Knowledge>();


            foreach (var kbItem in kbResultSets)
            {
                string knowledgeBaseSysId = string.Empty;
                string categorySysId = string.Empty;
                var kb = new Knowledge();

                if (FilterReqsFromPoliceDept(kbItem, "News"))
                    continue;

                if (kbItem["kb_knowledge_base"] != null)
                {
                    //knowledgeBaseSysId = kbItem["kb_knowledge_base"].SelectToken("value").ToString();
                    var knowledgeBaseURL = kbItem["kb_knowledge_base"].SelectToken("link").ToString();
                    knowledgeBaseSysId = Regex.Match(knowledgeBaseURL,
                                       string.Format("{0}/kb_knowledge_base/(.+)", ConfigurationManager.AppSettings["APIGenericURL"]),
                                       RegexOptions.Singleline).Groups[1].Value;
                }

                if (kbItem["kb_category"] != null)
                {
                    //categorySysId = kbItem["kb_category"].SelectToken("value").ToString();
                    var categoryURL = kbItem["kb_category"].SelectToken("link").ToString();
                    categorySysId = Regex.Match(categoryURL,
                                       string.Format("{0}/kb_category/(.+)", ConfigurationManager.AppSettings["APIGenericURL"]),
                                       RegexOptions.Singleline).Groups[1].Value;
                }

                if (knowledgeBaseSysId != kb.KBNewsOutagesKnowledgeBase)
                    continue;

                if (categorySysId != kb.KBNewsCategory)
                    continue;

                if (!string.IsNullOrWhiteSpace(kbItem["valid_to"].ToString()))
                {
                    if (Convert.ToDateTime(kbItem["valid_to"].ToString()).Date >= DateTime.Now.Date)
                    {
                        kb = new Knowledge();
                        kb.CreatedDt = Convert.ToString(kbItem["sys_created_on"]).Trim();
                        kb.Description = HttpUtility.HtmlDecode(Convert.ToString(kbItem["text"]).Trim());
                        kb.KBNum = Convert.ToString(kbItem["number"]).Trim();
                        kb.ShortDescription = Convert.ToString(kbItem["short_description"]).Trim();
                        kb.SysId = Convert.ToString(kbItem["sys_id"]).Trim();
                        kb.WorkflowStatus = Convert.ToString(kbItem["workflow_state"]).Trim();
                        kb.KBLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=kb_knowledge.do?sys_id=" + kb.SysId;
                        kb.PublishedDt = Convert.ToString(kbItem["published"]).Trim();
                        kb.ValidToDt = Convert.ToString(kbItem["valid_to"]).Trim();
                        kbList.Add(kb);
                    }
                }
            }

            if (kbList.Count <= 0)
            {
                // If no records are returned, create default record
                var kb = new Knowledge();
                kb.KBNum = "NWSNoChanges";
                kb.CreatedDt = string.Empty;
                kb.Description = string.Empty;
                kb.ShortDescription = "There are no news items at this time.";
                kb.SysId = string.Empty;
                //kb.WorkflowStatus = string.Empty;
                kb.WorkflowStatus = "published";
                kb.KBLink = string.Empty;
                kb.PublishedDt = string.Empty;
                kb.ValidToDt = string.Empty;
                kbList.Add(kb);
            }

            return kbList;
        }

        public List<Change> GetChanges(string ChangeData)
        {
            JObject ChangeObj = JObject.Parse(ChangeData);
            JArray ChangeResultSets = (JArray)ChangeObj["result"];
            List<Change> ChangeList = new List<Change>();

            // Loop through results and assign desired fields to Change object and add to list
            foreach (var ChangeItem in ChangeResultSets)
            {
                var change = new Change();
                string chgOutageStartDT = "";
                string chgOutageEndDT = "";
                string chgApprovalStatus = Convert.ToString(ChangeItem["approval"]).Trim();
                string chgState = Convert.ToString(ChangeItem["state"]).Trim();
                string chgETA = Convert.ToDateTime(ChangeItem["end_date"]).ToString("MM/dd/yyyy hh:mm tt"); //Planned end date

                if (FilterReqsFromPoliceDept(ChangeItem, "Change"))
                    continue;              

                if (Convert.ToString(ChangeItem["u_chg_outage_start"]) != string.Empty) //Outage start date
                {
                    chgOutageStartDT = Convert.ToDateTime(ChangeItem["u_chg_outage_start"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                if (Convert.ToString(ChangeItem["u_chg_outage_end"]) != string.Empty) //Outage end date
                {
                    chgOutageEndDT = Convert.ToDateTime(ChangeItem["u_chg_outage_end"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                DateTime chgEndDate = new DateTime();
                if (Convert.ToString(ChangeItem["u_chg_outage_end"]) != string.Empty)
                {
                    chgEndDate = Convert.ToDateTime(ChangeItem["u_chg_outage_end"]);
                }

                // TODO: Test logic for filtering planned outages
                if (chgApprovalStatus == "Approved" && (chgState == "Ready" || chgState == "Work In Progress" ||
                    chgState == "Completed" || chgState == "Failed" || chgState == "Cancelled"))
                {
                   
                    var dateDiff = (chgOutageEndDT != "") ? (DateTime.Now - chgEndDate).TotalDays : 10;
                    if ((chgOutageEndDT == "" || dateDiff <= 1) ||
                        ((chgState == "Completed" || chgState == "Failed" || chgState == "Cancelled") && dateDiff <= 1))
                    {
                        change.ChangeNumber = Convert.ToString(ChangeItem["number"]).Trim();
                        change.ChangeOutageStartDT = chgOutageStartDT;
                        change.ChangeOutageEndDT = chgOutageEndDT;
                        if (change.ChangeOutageEndDT != string.Empty)
                        {
                            change.ChangeApprovalStatus = "System AVAILABLE";
                        }
                        else
                        {
                            change.ChangeApprovalStatus = chgApprovalStatus;
                        }
                        change.ChangeState = chgState;
                        change.ChangeShortDescription = HttpUtility.HtmlDecode(Convert.ToString(ChangeItem["short_description"]).Trim());
                        change.ChangeSysId = Convert.ToString(ChangeItem["sys_id"]).Trim();
                        change.ChangeLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=change_request.do?sys_id=" + change.ChangeSysId;
                        change.ChangeETA = chgETA;
                        ChangeList.Add(change);
                    }
                }
            }


            if (ChangeList.Count() <= 0)
            {
                // If no records are returned, create default record
                var change = new Change();
                change.ChangeNumber = "CHGNoChanges";
                change.ChangeOutageStartDT = "";
                change.ChangeOutageEndDT = "";
                change.ChangeApprovalStatus = "";
                change.ChangeState = "";
                change.ChangeShortDescription = "There are no Pre-planned outages scheduled at this time.";
                change.ChangeSysId = "";
                change.ChangeLink = "";
                change.ChangeETA = "";
                ChangeList.Add(change);
            }

            return ChangeList;
        }

        /// <summary>
        /// Find Department through Sys Id
        /// </summary>
        /// <param name="sysId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string GetDepartmentBySysId(string sysId, string fieldName)
        {
            string url = ConfigurationManager.AppSettings["APIGenericURL"];
            string viewName = string.Empty;
            switch (fieldName)
            {
                case "requested_by":
                case "opened_by":
                case "caller_id":
                    viewName = "sys_user";
                    break;
                case "cmdb_ci":
                    viewName = "cmdb_ci";
                    break;
            }

            string requestUrl = string.Format("{0}/{1}/{2}", url, viewName, sysId);

            ServiceNowRequest webRequest = new ServiceNowRequest(requestUrl, "GET");
            var data = webRequest.GetResponse();
            JObject obj = JObject.Parse(data);
            var item = obj["result"];
            if (item != null)
            {
                if (item.SelectToken("department").ToString() != string.Empty)
                {
                    return item.SelectToken("department").SelectToken("value").ToString();    
                }                
            }
            return string.Empty;
        }

        /// <summary>
        /// Filter out Configuration Item belongs to Police Department
        /// Filter out the user from Police Department
        /// Filter out the short description contains Police/LAPD/L.A.P.D/P.D
        /// Filter out the assignment groups - ITA-DASD-Police Applications Support and ITA-PSALAPD-Infra
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool FilterReqsFromPoliceDept(JToken item, string requestType)
        {
            var policeDeptSysId = "cd425a30db7b1e003fc6d1fcbf9619e9";
            
            //Configuration item
            string configItem = item["cmdb_ci"].SelectToken("link") != null ? item["cmdb_ci"].SelectToken("link").ToString() : null;
            if (configItem != null)
            {
                //find configuration item's sys id
                var configItemSysId = Regex.Match(configItem,
                                        string.Format("{0}/cmdb_ci/(.+)", ConfigurationManager.AppSettings["APIGenericURL"]),
                                        RegexOptions.Singleline).Groups[1].Value;

                //find department id
                var deptSysId = GetDepartmentBySysId(configItemSysId, "cmdb_ci");
                if (deptSysId == policeDeptSysId) //filter out the change request from Police Dept
                    return true;
            }
            string reqTypeField = string.Empty;
            switch (requestType)
            {
                case "Change":
                    reqTypeField = "requested_by";
                    break;
                case "Problem":
                    reqTypeField = "opened_by";
                    break;
                case "Incident":
                    reqTypeField = "caller_id";
                    break;
                case "News":
                    requestType = "author";
                    break;
            }
            //Request By
            var requestedByURL = (item[reqTypeField] != null) ? item[reqTypeField].SelectToken("link").ToString() : null;
          
            if (requestedByURL != null)
            {
                var userSysId = Regex.Match(requestedByURL,
                                        string.Format("{0}/sys_user/(.+)", ConfigurationManager.AppSettings["APIGenericURL"]),
                                        RegexOptions.Singleline).Groups[1].Value;
                
                var userDeptSysId = GetDepartmentBySysId(userSysId, reqTypeField);
                if (userDeptSysId == policeDeptSysId)
                    return true;
            }

            //Short Description and Description
            string[] patterns = new string[] { "LAPD", "L.A.P.D", "PD", "P.D", "Police" };
            var shortDesc = item["short_description"].ToString();
            var desc = item["description"].ToString();
            foreach (var matchPattern in patterns)
            {
                if (shortDesc.Contains(matchPattern))
                    return true;
            }

            foreach (var matchPattern in patterns)
            {
                if (desc.Contains(matchPattern))
                    return true;
            }

            //assignment group
            string[] groups = new string[] { "ITA-DASD-Police Applications Support", "ITA-PSALAPD-Infra" };
            var assignmentGroup = item["assignment_group"];
            if (assignmentGroup != null)
            {
                foreach (var g in groups)
                {
                    if (assignmentGroup.Contains(g))
                        return true;
                }
            }
            return false;
        }
    }

}