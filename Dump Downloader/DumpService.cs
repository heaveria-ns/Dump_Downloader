using HtmlAgilityPack;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Dump_Downloader
{
    public class DumpService
    {
        public static void Setup()
        {
            // Create necessary folders. Nothing happens if they already exist.
            Console.WriteLine("Getting started...\n" +
                              "Creating directories...");
            string path = Directory.GetCurrentDirectory();
            Directory.CreateDirectory($"{path}\\Share");
            Directory.CreateDirectory($"{path}\\Share\\Dumps");
            Directory.CreateDirectory($"{path}\\Share\\Dumps\\nations");
            Directory.CreateDirectory($"{path}\\Share\\Dumps\\regions");
        }

        public static (List<string>, List<string>) GetDumpsList(string nationOrRegion)
        {
            // Intro
            Console.WriteLine("Getting list of dumps from the source archive...");

            // Load/Get HTML from Archive Website
            string url;
            switch (nationOrRegion)
            {
                case "nations":
                    url = "https://nationstates.s3.amazonaws.com/nations_dump/index.html";
                    break;

                case "regions":
                    url = "https://nationstates.s3.amazonaws.com/regions_dump/index.html";
                    break;

                default:
                    throw new Exception();
            }
            var web = new HtmlWeb();
            var doc = web.Load(url);

            // Get dump names and dump url and adds them to lists.
            List<string> names = new List<string>();
            List<string> urls = new List<string>();
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li"))
            {
                names.Add(node.InnerText);
                urls.Add(node.ChildNodes["a"].Attributes["href"].Value);
            }

            // Cleanup dumpNames to the desired format of yyyy-mm-dd.xml.gz
            for (int i = 0; i < names.Count; i++)
            {
                names[i] = names[i].Substring(8).Split(".")[0].Replace("_", "-")+$"-{nationOrRegion}.xml.gz";
            }

            return (names, urls);
        }

        public static (List<string> dumpNames, List<string> dumpUrls) CheckForExistingDumps(List<string> dumpNames, List<string> dumpUrls, string nationOrRegion, string basepath)
        {
            // Intro
            Console.WriteLine("Checking for existing dump files...");
            // Creates list and adds all existing files into it.
            var currentDumps = new List<string>();
            foreach (var dump in Directory.GetFiles(Path.Join(basepath, nationOrRegion)))
            {
                currentDumps.Add(dump.Split(Path.DirectorySeparatorChar).Last());
            }
            foreach (var urlRemoval in currentDumps
                .Select((n, i) => dumpNames.FindIndex(dn => dn == n))
                .Where(i => i > -1)) 
            {
                dumpUrls.RemoveAt(urlRemoval);
            }
            dumpNames = dumpNames.Except(currentDumps).ToList();
            return (dumpNames, dumpUrls);
        }

        public static async Task DownloadDumps(List<string> targetDumpNames, List<string> remoteDumpNames, string nationOrRegion, string storagePath)
        {
            // Shell Progress Bar
            int totalTicks = remoteDumpNames.Count;
            using var _httpService = new HttpService();

            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593',
                ProgressBarOnBottom = true,
            };
            using var pbar = new ProgressBar(totalTicks, "Loading dumps", options);
            for (int i = 0; i < remoteDumpNames.Count; i++)
            {
                // Safety check before continuing
                if (targetDumpNames.Count != remoteDumpNames.Count)
                {
                    throw new Exception($"ERROR: The list lengths of dumpNames and dumpUrls do not match. Program will refuse to continue.Names: {targetDumpNames.Count}, Urls: {remoteDumpNames.Count}");
                }
                await _httpService.SendRequest($"https://nationstates.s3.amazonaws.com/{nationOrRegion}_dump/{remoteDumpNames[i]}", Path.Join(storagePath, nationOrRegion, targetDumpNames[i]));
                pbar.Tick($"Step {i + 1} of {totalTicks}");
            }
        }

        private static void GzToXML(string xmlPath, string saveLocation)
        {
            Stream stream = File.Open(xmlPath, FileMode.Open);
            using (var instream = new GZipStream(stream, CompressionMode.Decompress))
            using (var outputStream = new FileStream(saveLocation, FileMode.Append, FileAccess.Write))
                instream.CopyTo(outputStream);
        }
    }
}