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

                // Gets list of all current dumps. Item1 = names, Item2 = urls.
                Console.Write("Would you like to backup dumps for nation or region? (N/R): ");
                string nationOrRegion = Console.ReadLine().ToUpper();

                Console.Write("Please set a User-Agent: ");
                string uAgent = Console.ReadLine();

                DumpService.Setup();
                (List<string>, List<string>) dumpsList;
                switch (nationOrRegion)
                {
                    case "N":
                    case "NATION":
                        dumpsList = DumpService.GetDumpsList("nations");
                        nationOrRegion = "nations";
                        break;
                    case "R":
                    case "REGION":
                        dumpsList = DumpService.GetDumpsList("regions");
                        nationOrRegion = "regions";
                        break;
                    default:
                        Console.WriteLine("ERROR: Your answer was not one of the following: n, r, nation, or region.");
                        return;
                }

                Console.Write("Please enter base path: ");
                var storageBasePath = Console.ReadLine();

                // Check for existing dumps and return list of everything to get.
                dumpsList = DumpService.CheckForExistingDumps(dumpsList.Item1, dumpsList.Item2, nationOrRegion, storageBasePath);

                // Download Dumps
                await DumpService.DownloadDumps(dumpsList.Item1, dumpsList.Item2, uAgent, nationOrRegion, storageBasePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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