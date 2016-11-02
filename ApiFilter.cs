using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace Api_Stats_Client
{
    public class ApiFilter : IHttpModule
    {
        private ApiPublishThread thread = null;
        private static string ipAddress = null;
        private String apiPattern = "/sherpa/.*";
        private String serviceName = null;
        private String apiServer = null;
        private bool valid = false;
        private String token = null;
        private long referenceId = -1;
        private long watchThreshold = 10;

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += delegate (object source, EventArgs e)
            {
                HttpApplication httpApplication = (HttpApplication)source;
                HttpRequest request = httpApplication.Context.Request;
                string method = request.HttpMethod;
                string uri = request.Url.AbsoluteUri;

                this.apiPattern = ConfigurationManager.AppSettings["api-pattern"];
                this.serviceName = ConfigurationManager.AppSettings["service-name"];
                this.apiServer = ConfigurationManager.AppSettings["grokola-server"];
                this.token = ConfigurationManager.AppSettings["token"];
                string threshold = ConfigurationManager.AppSettings["watch-threshold"];
                string refid = ConfigurationManager.AppSettings["reference-id"];

                if (threshold != null)
                {
                    try
                    {
                        this.watchThreshold = Convert.ToInt64(threshold);
                    }
                    catch (FormatException ex)
                    {
                       Console.WriteLine("Watch-threshold must be an integer: " + ex);
                    }                    
                }
                if (token == null)
                {
                    Console.WriteLine("Integration token required for API filter to be enabled...");
                }

                if (refid == null)
                {
                    Console.WriteLine("Grokola Reference Id must be defined...");
                } 
                else
                {
                    this.referenceId = Convert.ToInt64(refid);
                }

                if (this.apiServer == null)
                {
                    Console.WriteLine("Server must be specified in ApiFilter config... API's will not be watched...");
                }

                this.valid = this.apiServer != null & this.referenceId >= 0 && this.token != null;

                if (thread == null && this.valid)
                {
                    thread = new ApiPublishThread();
                    thread.setServiceName(this.serviceName);
                    thread.setServer(this.apiServer);
                    thread.setReferenceId(this.referenceId);
                    thread.setThreshold(this.watchThreshold);
                    thread.run();
                    Console.WriteLine("API Watch Thread started, will publish API's for every " + this.watchThreshold + "encounters...");
                }

                long start = System.DateTime.Now.Millisecond;

                if (Regex.IsMatch(uri, apiPattern) && this.valid)
                {
                    thread.add(uri, method, System.DateTime.Now.Millisecond - start);
                }
            };
            context.EndRequest += delegate (object source, EventArgs e)
            {                
                                               
            };
        }                
    }
}


