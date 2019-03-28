using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitCWallet
{
    public class Modules
    {
        public void TryCommunicatewithBitCoinCore()
        {
            var data = RequestServer("getblockcount", new List<string>());
        }

        private static string RequestServer(string methodName, List<string> parameters) //Javascript Object Notation RPC cli
        {
            string ServerIP = "http://localhost.:8332";
            string UserName = "Andis";
            string password = "221286";
            string data = string.Empty;

            var webRequest = (HttpWebRequest)WebRequest.Create(ServerIP);
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";
            string respVal = string.Empty;
            JObject jsobj = new JObject();
            jsobj.Add(new JProperty("jsonrpc", "1.0"));
            jsobj.Add(new JProperty("id", "1"));
            jsobj.Add(new JProperty("method", methodName));

            var props = new JArray();
            foreach (var param in parameters)
                props.Add(param);

            jsobj.Add(new JProperty("params", props));
            string s = JsonConvert.SerializeObject(jsobj);
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            webRequest.ContentLength = byteArray.Length;
            Stream datastream = webRequest.GetRequestStream();
            datastream.Write(byteArray, 0, byteArray.Length);
            datastream.Flush();
            StreamReader streamReader = null;
            try
            {
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                respVal = streamReader.ReadToEnd();
                data = JsonConvert.DeserializeObject(respVal).ToString();
            }
            catch (Exception e)
            {
                return $"Error : {e.Message}";
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
            }
            return data;
        }

    }
}