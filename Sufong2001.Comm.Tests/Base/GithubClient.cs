using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sufong2001.Comm.Tests.Base
{
    public class GithubClient : IGithubClient
    {
        public GithubClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }

        public async Task<GithubUser> GetUserAsync(string userName)
        {
            var response = await Client.GetAsync($"/users/{Uri.EscapeDataString(userName)}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<GithubUser>();
        }
    }
}
