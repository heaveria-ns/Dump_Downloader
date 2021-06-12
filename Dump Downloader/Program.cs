using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dump_Downloader
{
    internal class Program
    {
        private static async Task Main()
        {
            try
            {
                Intro();
                Console.Write("Please enter base path: ");
                var storageBasePath = Console.ReadLine();
                await DownloadDumps("nations", storageBasePath);
                await DownloadDumps("regions", storageBasePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task DownloadDumps(string nationOrRegion, string storageBasePath)
        {
            // Gets list of all current dumps. Item1 = names, Item2 = urls.
            (List<string>, List<string>) dumpsList = DumpService.GetDumpsList(nationOrRegion);
            // Check for existing dumps and return list of everything to get.
            dumpsList = DumpService.CheckForExistingDumps(dumpsList.Item1, dumpsList.Item2, nationOrRegion, storageBasePath);

            // Download Dumps
            await DumpService.DownloadDumps(dumpsList.Item1, dumpsList.Item2, nationOrRegion, storageBasePath);
        }

        private static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Title = "Dump Downloader by Heaveria";
            Console.WriteLine("Dump Downloader");
            Console.ResetColor();
            Console.WriteLine($"Developed By: Heaveria{Environment.NewLine}" +
                $"Archive Sources: https://nationstates.s3.amazonaws.com/regions_dump/index.html" +
                $"{Environment.NewLine}" +
                $"                 https://nationstates.s3.amazonaws.com/nations_dump/index.html\n");
        }
    }
}