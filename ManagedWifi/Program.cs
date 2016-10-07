using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Geolocation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Finding location .... ");
            JArray macArray = new JArray();

            Process p = new Process();
            p.StartInfo.FileName = "netsh.exe";
            p.StartInfo.Arguments = "wlan show networks mode=bssid";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            output = output.Replace('\r', ' ');
            string[] lines = output.Split('\n');

            JObject jobj;
            foreach (string line in lines)
            {
                if (line.Contains("BSSID"))
                {
                    string[] token = { " : " };
                    string[] splitLine = line.Split(token, 2, StringSplitOptions.None);
                    string mac = splitLine[1].Trim().ToUpper();

                    jobj = new JObject();
                    jobj.Add("macAddress", mac);
                    macArray.Add(jobj);
                }
            }

            JObject wifiObj = new JObject();
            wifiObj.Add("wifiAccessPoints", macArray);

            GetLocation(wifiObj);

            Console.Read();
        }

        static async void GetLocation(JObject wifiObj)
        {
            string location = "";
            string apiKey = "";

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(wifiObj.ToString(Newtonsoft.Json.Formatting.None), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://www.googleapis.com/geolocation/v1/geolocate?key=" + apiKey, content);
                location = await response.Content.ReadAsStringAsync();
            }

            try
            {
                JObject jObj = JObject.Parse(location);
                var latitude = (string)jObj["location"]["lat"];
                var longitude = (string)jObj["location"]["lng"];
                var accuracy = (string)jObj["accuracy"];

                Console.Clear();
                Console.WriteLine(string.Format("Latitude: {0}", latitude));
                Console.WriteLine(string.Format("Longitude: {0}", longitude));
                Console.WriteLine(string.Format("Accuracy: {0}", accuracy));
            }
            catch (Exception) 
            {
                Console.WriteLine("Error parsing address");
            }
        }
    }
}
