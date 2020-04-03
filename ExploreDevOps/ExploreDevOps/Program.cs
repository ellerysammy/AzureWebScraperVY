using System;
using System.Collections.Generic;
using System.IO;

namespace ExploreDevOps
{
    class Program
    {
        static void Main(string[] args)
        {

            var ALMVictims = new List<string>();

            var scraper = new AzureWebScraper();

            var projects = scraper.GetProjectsFromAzure().GetAwaiter().GetResult();

            foreach (var project in projects)
            {
                Console.WriteLine($"Dont worry, I am not dead, I am running ALM Search Utility for {project.name} (takes a few minutes)...");
                var BuildsWithoutSonar = scraper.GetProjectsFromAlmSearch(project.name, "Run Code Analysis").GetAwaiter().GetResult();
                Console.WriteLine("Finished ALM Search Utility");

                var repos = scraper.GetReposFromAzure(project.name).GetAwaiter().GetResult();

                foreach (var repo in repos)
                {

                    var build = scraper.GetBuildDefinition(project.name, repo.name, "master").GetAwaiter().GetResult();
                    var buildName = build?.name ?? "No Validation Build";

                    if (build == null || BuildsWithoutSonar.Contains(build.id))
                    {
                        ALMVictims.Add($"{project.name},{repo.name},{buildName}");

                        var output = $"Victimized: {project.name} : {repo.name} : {buildName}";

                        Console.WriteLine(output);

                        WriteToTxt(output);
                    }
                }
                Console.WriteLine();
            }
        }

        private static void WriteToTxt(string output)
        {
            using (StreamWriter w = File.AppendText("./output.txt"))
            {
                w.WriteLine(output);
            }
        }
    }
}
