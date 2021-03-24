using System.Collections.Generic;
using Google.Protobuf.Collections;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.API.Utility;
using PluginJira.Helper;

namespace PluginJira.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetRefreshSchemas(IApiClient apiClient,
            RepeatedField<Schema> refreshSchemas, int sampleSize = 5)
        {
            foreach (var schema in refreshSchemas)
            {
                var endpoint = EndpointHelper.GetEndpointForSchema(schema);

                var refreshSchema = await GetSchemaForEndpoint(apiClient, schema, endpoint);

                // get sample and count
                yield return await AddSampleAndCount(apiClient, refreshSchema, sampleSize, endpoint);
            }
        }
    }
}