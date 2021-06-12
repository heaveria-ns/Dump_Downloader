using System;
using System.Collections.Generic;
using static Dump_Downloader.Dumps;

namespace Dump_Downloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Intro();
            
            // Gets list of all current dumps. Item1 = names, Item2 = urls.
            Console.Write("Would you like to backup dumps for nation or region? (N/R): ");
            string nationOrRegion = Console.ReadLine().ToUpper();
            
            Console.Write("Please set a User-Agent: ");
            string uAgent = Console.ReadLine();
            
            Setup();
            (List<string>, List<string>) dumpsList;
            switch (nationOrRegion)
            {
                case "N":
                    dumpsList = GetDumpsList("nations");
                    nationOrRegion = "nations";
                    break;
                case "NATION":
                    dumpsList = GetDumpsList("nations");
                    nationOrRegion = "nations";
                    break;
                case "R":
                    dumpsList = GetDumpsList("regions");
                    nationOrRegion = "regions";
                    break;
                case "REGION":
                    dumpsList = GetDumpsList("regions");
                    nationOrRegion = "regions";
                    break;
                default:
                {
                    Console.WriteLine(
                        "\nERROR: Your answer was not one of the following: n, r, nation, or region.");
                    throw new Exception();
                }
            }
            
            // Check for existing dumps and return list of everything to get.
            dumpsList = CheckForExistingDumps(dumpsList.Item1, dumpsList.Item2, nationOrRegion);
            
            // Download Dumps
            
            DownloadDumps(dumpsList.Item1, dumpsList.Item2, uAgent, nationOrRegion);
        }

        static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Dump Downloader");
            Console.ResetColor();
            Console.WriteLine("Developed By: Heaveria\n" +
                              "Archive Sources: https://nationstates.s3.amazonaws.com/regions_dump/index.html\n" +
                              "                 https://nationstates.s3.amazonaws.com/nations_dump/index.html\n");
        }
    }
}