using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Dump_Downloader
{
    public class Http
    {
        public static void ApiRequest(string url, string uAgent, string saveLocation)
        {
            try
            {
                // Setup HTTP Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("User-Agent", uAgent);
                request.Method = "GET";
                // Send Request
                var reqAnswer = request.GetResponse();
                using (Stream output = File.OpenWrite(saveLocation))
                using (Stream input = reqAnswer.GetResponseStream())
                {
                    input.CopyTo(output);
                }
                // 5s Ratelimit
                Thread.Sleep(5000);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nERROR: This file likely does not exist! Moving on...\n");
                Console.ResetColor();
            }
        }
    }
}