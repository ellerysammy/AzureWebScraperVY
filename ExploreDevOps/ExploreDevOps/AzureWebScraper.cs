using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ExploreDevOps
{
    public class AzureWebScraper
    {

        public async Task<IHtmlDocument> GetHtmlDocFromAzure(string project, string repoName, string branch = "master")
        {
            string personalAccessToken = "lhci6kpvinehsgbnov7qmgnul4oz2lnbmc7htq4pq27sg6cgy2tq";
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string siteUrl = $"https://dev.azure.com/vueling/{project}/_git/{repoName}/branchpolicies?refName=refs%2Fheads%2F{branch}";
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);
            return document;
        }

        public string GetJsonStringFromHtml(IHtmlDocument document)
        {
            string jsonResult = string.Empty;
            var dataProvider = document.Scripts?.FirstOrDefault(x => x.Id == "dataProviders");
            if (dataProvider != null)
            {
                jsonResult = dataProvider.InnerHtml;
            }
            return jsonResult;
        }

        public async Task<string> GetJsonFromAzure(string url)
        {
            string personalAccessToken = "lhci6kpvinehsgbnov7qmgnul4oz2lnbmc7htq4pq27sg6cgy2tq";
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string siteUrl = url;
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();
            
            StreamReader reader = new StreamReader(response);
            string text = reader.ReadToEnd();
            return text;
        }

        public async Task<List<int>> GetProjectsFromAlmSearch(string projectName, string textToSearch)
        {
            var almSearchUrl = "https://privateint.vueling.com/Vueling.ALM.Validations.WebApi/api/v1/PipelinesChecks/search";

            var request = new AlmSearchRequest()
            {
                ExpressionToFind = textToSearch,
                TeamProject = projectName,
                CheckInJson = false
            };

            var json = JsonConvert.SerializeObject(request);
            var data = new StringContent(json, Encoding.UTF8, "application/json");


            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            var result = await httpClient.PostAsync(almSearchUrl,data);

            Stream response = await result.Content.ReadAsStreamAsync();
            
            StreamReader reader = new StreamReader(response);
            string text = reader.ReadToEnd();

            var urlList = JsonConvert.DeserializeObject<List<string>>(text);

            return urlList.Select(url => GetBuildIdFromEditLink(url)).ToList();
        }

        private int GetBuildIdFromEditLink(string editLink)
        {
            try
            {
                var query = editLink.Split("?")[1];
                var queryMembers = query.Split("&");
                foreach (var member in queryMembers)
                {
                    if (member.StartsWith("id="))
                    {
                        var number = member.Split("=")[1];
                        return int.Parse(number);
                    }
                }
                return 0;
            }
            catch 
            {
                return 0;
            }
        }
        public async Task<List<Project>> GetProjectsFromAzure()
        {
            var projectsUrl = "https://dev.azure.com/vueling/_apis/projects";
            var json = await GetJsonFromAzure(projectsUrl);

            var projects = JsonConvert.DeserializeObject<Projects>(json);
            return projects.value;
        }

        public async Task<List<Repo>> GetReposFromAzure(string projectName)
        {
            var encodedProjectName = projectName.ToLower().Replace(" ", "%20");
            var reposUrl = $"https://dev.azure.com/vueling/{encodedProjectName}/_apis/git/Repositories";

            var json = await GetJsonFromAzure(reposUrl);

            var repos = JsonConvert.DeserializeObject<Repos>(json);
            return repos.value;
        }

        public async Task<BuildDefinition> GetBuildDefinition(string projectName, string repoName, string branchName)
        {
            var doc = await GetHtmlDocFromAzure(projectName, repoName, branchName);
            var data = GetJsonStringFromHtml(doc);
            var jsonData = JObject.Parse(data);

            if (jsonData.TryGetValue("data", out JToken value))
            {
                var policies = JObject.Parse(value.Value<JObject>("ms.vss-code-web.admin-policies-data-provider").ToString());
                if (policies.TryGetValue("buildDefinitions", out JToken definitions))
                {
                    if (definitions != null)
                    {
                        var arr = JArray.Parse(definitions.ToString());
                        var bdef = arr.ToObject<List<BuildDefinition>>();
                        if (bdef.Any())
                        {
                            return bdef.First();
                        }
                    }
                }
            }
            return null;
        }
    }
}
