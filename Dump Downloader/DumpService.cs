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
        public static Dictionary<string, string> GetDumpsList(string nationOrRegion)
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
                names[i] = names[i].Substring(8).Split(".")[0].Replace("_", "-") + $"-{nationOrRegion}.xml.gz";
            }

            return names.Zip(urls, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        public static Dictionary<string, string> CheckForExistingDumps(Dictionary<string, string> namesAndUrls, string nationOrRegion, string basepath)
        {
            // Intro
            Console.WriteLine("Checking for existing dump files...");
            // Creates list and adds all existing files into it.
            var dic = namesAndUrls;
            var currentDumps = new List<string>();
            foreach (var dump in Directory.GetFiles(Path.Join(basepath, nationOrRegion)))
            {
                currentDumps.Add(dump.Split(Path.DirectorySeparatorChar).Last());
            }
            var neededNames = dic.Keys.Except(currentDumps).ToList();
            dic.RemoveAll((key, value) => !neededNames.Contains(key));
            return dic;
        }

        public static async Task DownloadDumps(Dictionary<string, string> namesAndUrls, string nationOrRegion, string storagePath)
        {
            // Shell Progress Bar
            int totalTicks = namesAndUrls.Count;
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
            int counter = 0;
            foreach (var nameAndUrl in namesAndUrls)
            {
                await _httpService.SendRequest($"https://nationstates.s3.amazonaws.com/{nationOrRegion}_dump/{nameAndUrl.Value}", Path.Join(storagePath, nationOrRegion, nameAndUrl.Key));
                counter++;
                pbar.Tick($"Step {counter} of {totalTicks}");
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

    public static class DictionaryExt
    {
        public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> predicate)
        {
            foreach (var key in dict.Keys.ToArray().Where(key => predicate(key, dict[key])))
                dict.Remove(key);
        }
    }
}