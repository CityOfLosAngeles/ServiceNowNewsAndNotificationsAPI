﻿using System;
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
                string kbURL = "https://cityoflaprod.service-now.com/api/now/table/kb_knowledge?sysparm_query=Title%3D%27News%20and%20Outages%27^topic=News&sysparm_limit=10";                                

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
    }
}
