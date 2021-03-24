using System;
using System.Net.Http;
using System.Threading.Tasks;
using PluginJira.Helper;

namespace PluginJira.API.Factory
{
    public class ApiAuthenticator: IApiAuthenticator
    {
        private HttpClient Client { get; set; }
        private Settings Settings { get; set; }
        
        public ApiAuthenticator(HttpClient client, Settings settings)
        {
            Client = client;
            Settings = settings;
        }

        //get token
        public Task<string> GetToken()
        {
            // build basic auth token
            var authenticationString = $"{Settings.Username}:{Settings.ApiKey}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
            return Task.FromResult(base64EncodedAuthenticationString);
        }

        private Task<string> GetNewToken()
        {
            return Task.FromResult(Settings.ApiKey);
        }

        


    }
}