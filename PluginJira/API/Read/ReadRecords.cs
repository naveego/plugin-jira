using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.API.Utility;
using PluginJira.Helper;

namespace PluginJira.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClientFactory factory, Settings settings, Schema schema, DateTime? lastReadTime = null, TaskCompletionSource<DateTime>? tcs = null)
        {
            var endpoint = EndpointHelper.GetEndpointForSchema(schema);

            var records = endpoint?.ReadRecordsAsync(factory, settings, lastReadTime, tcs);

            if (records != null)
            {
                await foreach (var record in records)
                {
                    yield return record;
                }
            }
        }
    }
}