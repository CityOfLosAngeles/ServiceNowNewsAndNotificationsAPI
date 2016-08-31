using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using ServiceNowNewsAndNotificationsAPI.Models;
using System.Web.Script.Serialization;


namespace ServiceNowNewsAndNotificationsAPI.Controllers
{
    public class OutageController : ApiController
    {
        [HttpGet]       
        [Route("getProblems")]
        public HttpResponseMessage getProblems()
        {
            try
            {
                //Incident URL
                string incidentURL = "https://cityoflaprod.service-now.com/api/now/table/incident?active=true&u_outage=true";
                
                //Problem URL
                string problemURL = "https://cityoflaprod.service-now.com/api/now/table/problem?active=true&u_outage=true";

                // Incident Connection
                ServiceNowRequest webRequest = new ServiceNowRequest(incidentURL, "GET");
                var incidentData = webRequest.GetResponse();

                webRequest = new ServiceNowRequest(problemURL, "GET");
                var problemData = webRequest.GetResponse();

                var getData = new ProcessData();
                var list = getData.DataMassage(incidentData, problemData);

                var json = new JavaScriptSerializer().Serialize(list);

                return Request.CreateResponse(HttpStatusCode.OK, json);         
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.InnerException +":" + e.StackTrace + ":" + e.Message);
            }
        }
    }
}
