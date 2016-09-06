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
            JObject incidentsObj = JObject.Parse(incidentData);
            JArray incidentArray = (JArray)incidentsObj["result"];
            List<Incident> incidentList = new List<Incident>();
            List<Problem> problemList = new List<Problem>();

            for (var i = 0; i < incidentArray.Count; i++)
            {
                var rec = incidentArray[i];
                var inc = new Incident();
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

                string incStartDate = "";
                if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                {
                    incStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                string incEndDate = "";
                if (Convert.ToString(rec["u_outage_end_dttm"]) != string.Empty)
                {
                    incEndDate = Convert.ToDateTime(rec["u_outage_end_dttm"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                string incCreatedDate = "";
                if (Convert.ToString(rec["opened_at"]) != string.Empty)
                {
                    incCreatedDate = Convert.ToDateTime(rec["opened_at"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                string incClosedDate = "";
                if (Convert.ToString(rec["closed_at"]) != string.Empty)
                {
                    incClosedDate = Convert.ToDateTime(rec["closed_at"]).ToString("MM/dd/yyyy hh:mm tt");
                }

                string outageStartDate = "";
                string outageStartTime = "";
                if (Convert.ToString(rec["u_outage_start_dttm"]) != string.Empty)
                {
                    outageStartDate = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("MM/dd/yyyy");
                    outageStartTime = Convert.ToDateTime(rec["u_outage_start_dttm"]).ToString("hh:mm tt");
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
                inc.ClosedDt = (incClosedDate != string.Empty) ? incClosedDate : ""; //closed date
                inc.IncidentLink = "https://cityoflaprod.service-now.com/nav_to.do?uri=incident.do?sys_id=" + Convert.ToString(rec["sys_id"]);
                incidentList.Add(inc);
            }

            JObject problemsObj = JObject.Parse(problemData);
            JArray problemArray = (JArray)problemsObj["result"];

            string prbStartDate = "";
            string prbEndDate = "";
            string prbCreatedDate = "";

            for (var i = 0; i < problemArray.Count; i++)
            {
                var rec = problemArray[i];

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

            //Build Problem and Incident RelationShip
            foreach (var p in problemList)
            {
                //find related incident
                var relatedIncs = incidentList.Where(x => x.ProblemId == p.SysId);
                if (relatedIncs.Count() > 0)
                {
                    foreach (var inc in relatedIncs)
                    {
                        inc.ProblemNum = p.ProblemNum;
                    }
                   p.Incidents.AddRange(relatedIncs);
                }
            }

            // TODO: Add Incident at top level to problemList
            var unrelatedIncs = incidentList.Where(w => w.ProblemId == "");
            if (unrelatedIncs.Count() > 0)
            {
                problemList.Add(new Problem()
                {
                    ProblemNum = "PRBNOPROBLEM",
                    OutageStartDateTime = "",
                    OutageEndDateTime = "",
                    OutageScope = "",
                    OutageType = "",
                    OutageStatus = "",
                    ShortDescription = "",
                    CreatedDt = "",
                    SysId = "",
                    ProblemLink = "",                    
                    Incidents = unrelatedIncs.ToList()
                });
            }
                    
            //Assign unrelated incidents' problem num
            foreach (var unrelatedinc in unrelatedIncs)
            {
                unrelatedinc.ProblemNum = "PRBNOPROBLEM";
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
                    if (Convert.ToDateTime(kbItem["valid_to"].ToString()).Date >= DateTime.Now.Date)
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
              
         

            return kbList;
        }
    }
}