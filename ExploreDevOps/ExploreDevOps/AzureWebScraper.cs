using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExploreDevOps
{
    public class AzureWebScraper
    {

        public async Task<IHtmlDocument> GetHtmlDocFromAzure(string repoName, string project)
        {
            string personalAccessToken = "lhci6kpvinehsgbnov7qmgnul4oz2lnbmc7htq4pq27sg6cgy2tq";
            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));
            string siteUrl = $"https://dev.azure.com/vueling/{project}/_git/{repoName}/branchpolicies?refName=refs%2Fheads%2Fmaster";
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
    }
}
