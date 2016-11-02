using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Api_Stats_Client
{
    public class ApiPublishThread
    {
        private Stack<String[]> apis = new Stack<string[]>();
        private string serviceName = null;
        private string grokola = "http://127.0.0.1:8080/sherpa/api/stats/test";
        private long threshold = 10;
        private string server;
        private long referenceId = 0;
        private string uri = "sherpa/api/stats";
        private string token = null;

        public string getToken()
        {
            return token;
        }

        public void setToken(string token)
        {
            this.token = token;
        }

        public long getReferenceId()
        {
            return referenceId;
        }

        public void setReferenceId(long referenceId)
        {
            this.referenceId = referenceId;
        }

        public string getServer()
        {
            return server;
        }

        public void setServer(string server)
        {
            this.server = server;
        }

        public long getThreshold()
        {
            return threshold;
        }

        public void setThreshold(long threshold)
        {
            this.threshold = threshold;
        }


        public string generateServiceName()
        {

            if (serviceName == null)
            {
                serviceName = hostName();
            }

            return serviceName;
        }

        public string getServiceName()
        {
            return this.serviceName;
        }

        public void setServiceName(string serviceName)
        {
            this.serviceName = serviceName;
        }

        public void add(string api, string method, long milliseconds)
        {
            apis.Push(new string[] { api, method, "" + milliseconds });
        }

        public void emit()
        {
            string request = this.server + "/" + this.uri + "/" + this.referenceId;

            try
            {
                var myUri = new Uri(request);
                WebRequest wReq = WebRequest.Create(myUri);
                HttpWebRequest httpReq = (HttpWebRequest)wReq;
                httpReq.Method = "POST";
                httpReq.ContentType = "application/json";
                httpReq.Headers.Add("Authorization", "Bearer " + this.token);

                string postDataStr = json();                
                byte[] postData = Encoding.UTF8.GetBytes(postDataStr);    
                            
                httpReq.ContentLength = postData.Length;

                Stream dataStream = httpReq.GetRequestStream();
                dataStream.Write(postData, 0, postData.Length);
                dataStream.Close();

                WebResponse response = httpReq.GetResponse();

                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                Console.WriteLine(responseFromServer);

                reader.Close();
                dataStream.Close();
                response.Close();
                //conn.setRequestProperty("token", this.token);

                Console.WriteLine("API Stats published");
            }
            catch (IOException e)
            {
                Console.WriteLine("Error accessing: " + grokola);
                Console.WriteLine(e.ToString());
            }

        }

        private string hostName()
        {
            IPAddress ip = null;
            string hostName = null;
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                if (host != null)
                {
                    return host.HostName;
                }                                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting hostname");
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        public String json()
        {
            StringBuilder json = new StringBuilder();
            long size = apis.Count();
            json.Append("[");
            while (size > 0)
            {
                if (apis.Count() != 0)
                {
                    String[] v = apis.Pop();
                    json.Append("{uri:\"" + v[0] + "\", method: \"" + v[1] + "\", duration: \"" + v[2] + "\", service: \"" + this.generateServiceName() + "\"}");
                    size = apis.Count();
                    if (size > 0) { json.Append(","); }
                }
            }
            json.Append("]");

            Console.WriteLine("JSON - " + json.ToString());

            return json.ToString();
        }
        
        public void run()
        {
            while (true)
            {

                if (apis.Count() > threshold)
                {
                    this.emit();
                }

                try
                {
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }
}
