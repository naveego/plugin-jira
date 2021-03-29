using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginJira.API.Factory;
using PluginJira.API.Utility;
using PluginJira.Helper;

namespace PluginJira.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClientFactory factory, Settings settings,
            int sampleSize = 5)
        {
            var allEndpoints = EndpointHelper.GetAllEndpoints();

            foreach (var endpoint in allEndpoints.Values)
            {
                // base schema to be added to
                var schema = new Schema
                {
                    Id = endpoint.Id,
                    Name = endpoint.Name,
                    Description = "",
                    PublisherMetaJson = JsonConvert.SerializeObject(endpoint),
                    DataFlowDirection = endpoint.GetDataFlowDirection()
                };

                schema = await GetSchemaForEndpoint(factory, settings, schema, endpoint);

                // get sample and count
                yield return await AddSampleAndCount(factory, settings, schema, sampleSize, endpoint);
            }
        }

        private static async Task<Schema> AddSampleAndCount(IApiClientFactory factory, Settings settings, Schema schema,
            int sampleSize, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }

            // add sample and count
            var records = Read.Read.ReadRecordsAsync(factory, settings, schema).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());
            schema.Count = await GetCountOfRecords(factory, settings, endpoint);

            return schema;
        }

        private static async Task<Schema> GetSchemaForEndpoint(IApiClientFactory factory, Settings settings,Schema schema, Endpoint? endpoint)
        {
            if (endpoint == null)
            {
                return schema;
            }

            if (endpoint.ShouldGetStaticSchema)
            {
                return await endpoint.GetStaticSchemaAsync(factory, settings, schema);
            }

            var recordsListRaw = await endpoint.ReadRecordsAsync(factory, settings, null, null, true).Take(100).ToListAsync();
            var recordsList = recordsListRaw
                .Select(r => JsonConvert.DeserializeObject<Dictionary<string, object>>(r.DataJson))
                .ToList();

            var types = GetPropertyTypesFromRecords(recordsList);

            var record = recordsList.FirstOrDefault();

            var properties = new List<Property>();

            if (record != null)
            {
                foreach (var recordKey in record.Keys)
                {
                    var property = new Property
                    {
                        Id = recordKey,
                        Name = recordKey,
                        Type = types[recordKey],
                        IsKey = endpoint.PropertyKeys.Contains(recordKey),
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = await endpoint.IsCustomProperty(factory, settings, recordKey)
                            ? Constants.CustomProperty
                            : "",
                        IsNullable = true
                    };

                    properties.Add(property);
                }
            }

            schema.Properties.Clear();
            schema.Properties.AddRange(properties);

            if (schema.Properties.Count == 0)
            {
                schema.Description = Constants.EmptySchemaDescription;
            }

            return schema;
        }
    }
}