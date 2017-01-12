using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using ServiceNowNewsAndNotificationsAPI.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json;


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

           // if (problemArray.Count > 0)
            {
                // Get Incident data from ServiceNow
                JObject incidentsObj = JObject.Parse(incidentData);
                JArray incidentArray = (JArray)incidentsObj["result"];

                for (var i = 0; i < incidentArray.Count; i++)
                {
                    var rec = incidentArray[i];
                    string statusValue = "";
                    switch (Convert.ToString(rec["state"]))
                    {
                        case "1":
                            statusValue = "New";
                            break;
                        case "2":
                            statusValue = "Active";
                            break;
                        case "3":
                            statusValue = "Awaiting Problem";
                            break;
                        case "4":
                            statusValue = "Awaiting User Info";
                            break;
                        case "6":
                            statusValue = "Resolved";
                            break;
                        case "7":
                            statusValue = "Closed";
                            break;
                    }


                    //check incident closed date if within 24hr then show
                    DateTime incClosedDate = new DateTime();
                    if (Convert.ToString(rec["closed_at"]) != string.Empty)
                    {
                        incClosedDate = Convert.ToDateTime(rec["closed_at"]).ToLocalTime();
                    }

                    var dateDiff = (DateTime.Now - incClosedDate).TotalDays;
                    if (string.IsNullOrWhiteSpace(rec["closed_at"].ToString()) || dateDiff <= 1)
                    {
                        //create new incident holder
                        var inc = new Incident();

                        string incStartDate = "";
                        if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                        {
                            incStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                        }

                        string incEndDate = "";
                        if (Convert.ToString(rec["u_outage_end_dttm"]) != string.Empty)
                        {
                            incEndDate = Convert.ToDateTime(rec["u_outage_end_dttm"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                        }

                        string incCreatedDate = "";
                        if (Convert.ToString(rec["opened_at"]) != string.Empty)
                        {
                            incCreatedDate = Convert.ToDateTime(rec["opened_at"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                        }



                        string outageStartDate = "";
                        string outageStartTime = "";
                        if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                        {
                            outageStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToLocalTime().ToString("MM/dd/yyyy");
                            outageStartTime = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToLocalTime().ToString("hh:mm tt");
                        }

                        inc.IncidentNum = Convert.ToString(rec["number"]); //incident number
                        inc.IncidentStatus = (statusValue == "Awaiting Problem") ? "Related" : statusValue; //New, Active, Awaiting Problem, Awawiting User Info, Resolved, Closed
                        inc.OutageStartDateTime = (incStartDate != string.Empty) ? incStartDate : ""; //outage start datetime
                        inc.OutageEndDateTime = (incEndDate != string.Empty) ? incEndDate : "";//outage end datetime
                        inc.OutageScope = Convert.ToString(rec["u_outage_scope"]); //outage scope [critical or not]
                        inc.OutageType = Convert.ToString(rec["u_outage_type"]); //Type of Outage (Planned, Unplanned or Awaiting Problem)
                        inc.ShortDescription = Convert.ToString(rec["short_description"]); //short description
                        inc.CreatedDt = incCreatedDate; //open date
                        inc.OutageStartDt = outageStartDate;
                        inc.OutageStartTm = outageStartTime;
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

                // Set Problem data with related Incidents
                for (var i = 0; i < problemArray.Count; i++)
                {
                    var rec = problemArray[i];
                    string prbStartDate = "";
                    string prbEndDate = "";
                    string prbCreatedDate = "";

                    if (Convert.ToString(rec["u_outage_start_date_time"]) != string.Empty)
                    {
                        prbStartDate = Convert.ToDateTime(rec["u_outage_start_date_time"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                    }
                    else
                    {
                        prbStartDate = "";
                    }

                    if (Convert.ToString(rec["u_outage_end_date_time"]) != string.Empty)
                    {
                        prbEndDate = Convert.ToDateTime(rec["u_outage_end_date_time"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                    }
                    else
                    {
                        prbEndDate = "";
                    }

                    if (Convert.ToString(rec["opened_at"]) != string.Empty)
                    {
                        prbCreatedDate = Convert.ToDateTime(rec["opened_at"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
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
                        probEndDate = Convert.ToDateTime(rec["u_outage_end_date_time"]).ToLocalTime();
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
                            OutageType = Convert.ToString(rec["u_outage_type"]), //Type of Outage (Planned, Unplanned or Awaiting Problem)
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

                    //find related incident
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
                    OutageType = "",
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
                if (!string.IsNullOrWhiteSpace(kbItem["valid_to"].ToString()))
                {
                    if (Convert.ToDateTime(kbItem["valid_to"].ToString()).ToLocalTime().Date >= DateTime.Now.Date)
                    {
                        var kb = new Knowledge();
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
                string chgETA = Convert.ToDateTime(ChangeItem["end_date"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");

                if (Convert.ToString(ChangeItem["u_chg_outage_start"]) != string.Empty)
                {
                    chgOutageStartDT = Convert.ToDateTime(ChangeItem["u_chg_outage_start"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                }

                if (Convert.ToString(ChangeItem["u_chg_outage_end"]) != string.Empty)
                {
                    chgOutageEndDT = Convert.ToDateTime(ChangeItem["u_chg_outage_end"]).ToLocalTime().ToString("MM/dd/yyyy hh:mm tt");
                }

                DateTime chgEndDate = new DateTime();
                if (Convert.ToString(ChangeItem["u_chg_outage_end"]) != string.Empty)
                {
                    chgEndDate = Convert.ToDateTime(ChangeItem["u_chg_outage_end"]).ToLocalTime();
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
    }
    
}