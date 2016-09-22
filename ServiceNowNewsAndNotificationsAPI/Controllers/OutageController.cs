using System;
using System.Collections.Generic;
using System.Configuration;
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
                
                string incidentURL = ConfigurationManager.AppSettings["IncidentcURL"];

                //Problem URL
                
                string problemURL = ConfigurationManager.AppSettings["ProblemcUrl"];
                // Incident Connection
                ServiceNowRequest webRequest = new ServiceNowRequest(incidentURL, "GET");
                var incidentData = webRequest.GetResponse();

                webRequest = new ServiceNowRequest(problemURL, "GET");
                var problemData = webRequest.GetResponse();

                var getData = new ProcessData();
                var list = getData.DataMassage(incidentData, problemData);              

                return Request.CreateResponse(HttpStatusCode.OK, list);         
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.InnerException +":" + e.StackTrace + ":" + e.Message);
            }
        }


        [HttpGet]       
        [Route("getNews")]       
        public HttpResponseMessage getNews()
        {
            try
            {
                //Knowledge URL
                string kbURL = ConfigurationManager.AppSettings["NewcURL"];                                

                // Knowledge Connection
                ServiceNowRequest webRequest = new ServiceNowRequest(kbURL, "GET");
                var kbData = webRequest.GetResponse();                

                var getData = new ProcessData();
                var list = getData.GetNews(kbData);              

                return Request.CreateResponse(HttpStatusCode.OK, list);         
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.InnerException +":" + e.StackTrace + ":" + e.Message);
            }
        }


        [HttpGet]
        [Route("getChanges")]
        public HttpResponseMessage getChanges()
        {
            try
            {
                //Knowledge URL
                string ChangeURL = ConfigurationManager.AppSettings["ChangecURL"];

                // Knowledge Connection
                ServiceNowRequest webRequest = new ServiceNowRequest(ChangeURL, "GET");
                var ChangeData = webRequest.GetResponse();

                var getData = new ProcessData();
                var list = getData.GetChanges(ChangeData);

                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.InnerException + ":" + e.StackTrace + ":" + e.Message);
            }
        }
    }
}
