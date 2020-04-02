using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ExploreDevOps
{
    public class AzureAccess
    {
        public static async Task<List<object>> GetPipelines()
        {
            List<object> result = new List<object>();
            string str_result = string.Empty;
            string personalAccessToken = "lhci6kpvinehsgbnov7qmgnul4oz2lnbmc7htq4pq27sg6cgy2tq";
            ////string url = "https://dev.azure.com/vueling";

            string credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken)));

            ////Uri uri = new Uri(url);
            ////VssBasicCredential credentials = new VssBasicCredential("", personalAccessToken);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://dev.azure.com/vueling"); 
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                HttpResponseMessage response = client.GetAsync("/ams/_apis/build/definitions?stateFilter=All").Result;

                if (response.IsSuccessStatusCode)
                {
                    str_result = await response.Content.ReadAsStringAsync();
                }
            }

            return result;
        }
    }
}
