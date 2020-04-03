using System;
using System.Collections.Generic;
using System.IO;

namespace ExploreDevOps
{
    class Program
    {
        public static string localPath = "./output_csv.csv";

        static void Main(string[] args)
        {
            var ALMVictims = new List<string>();

            var scraper = new AzureWebScraper();

            var projects = scraper.GetProjectsFromAzure().GetAwaiter().GetResult();

            var checkProject = false;

            CreateFile();

            Console.Write("Enter the project you want to start checking from: ");
            var startProject = Console.ReadLine();


            if (startProject == "") checkProject = true;

            foreach (var project in projects)
            {
                if (project.name == startProject || checkProject)
                {
                    checkProject = true;

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

                            Line line = new Line(project?.name, repo?.name, buildName);
                            AddToCsv(line);
                        }
                    }
                    Console.WriteLine();

                }
            }

            Console.Read();
        }

        public static void CreateFile()
        {
            File.WriteAllBytes(localPath, new byte[] { 0 });

            var csvHeaders = string.Format("{0},{1},{2}{3}", "Project", "Repository", "Build", Environment.NewLine);

            File.AppendAllText(localPath, csvHeaders);
        }

        private static void AddToCsv(Line line)
        {
            var newLine = string.Format("{0},{1},{2}{3}", line.Project, line.Repository, line.Build, Environment.NewLine);

            File.AppendAllText(localPath, newLine);
        }
    }

    public class Line
    {
        public string Project { get; set; }
        public string Repository { get; set; }
        public string Build { get; set; }

        public Line(string Project, string Repository, string Build)
        {
            this.Project = Project;
            this.Repository = Repository;
            this.Build = Build;
        }
    }
}
