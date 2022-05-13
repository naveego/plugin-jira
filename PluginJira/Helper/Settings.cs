using System;
using System.Collections.Generic;

namespace PluginJira.Helper
{
    public class Settings
    {
        
        public string Username { get; set; }
        public string ApiKey { get; set; }
        public string Tenant { get; set; }
        public string Depth { get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            // if (string.IsNullOrWhiteSpace(ApiKey))
            // {
            //     throw new Exception("The Api Key property must be set");
            // }
            if (string.IsNullOrWhiteSpace(Username))
            {
                throw new Exception("The username is not set properly");
            }

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new Exception("The Api Key property must be set");
            }

             if (string.IsNullOrWhiteSpace(Tenant))
            {
                throw new Exception("The Tenant is not set properly");
            }

             if (string.IsNullOrWhiteSpace(Depth))
             {
                 throw new Exception("The depth property must be set");
             }

             if (Int32.Parse(Depth) < 0)
             {
                 throw new Exception("The depth property must be an integer 0 or greater");
             }
        }

        public string GetSdkUri() 
        {
            return $"https://{Tenant}.atlassian.net";     
        }

        public string GetBaseUri()
        {
            return $"https://{Tenant}.atlassian.net/rest/api/2";           
        }
    }
}