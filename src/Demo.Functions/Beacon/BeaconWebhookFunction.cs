using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Demo.Functions.Completed;
using Demo.Functions.Logging;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Demo.Functions.Beacon
{
    public static class BeaconWebhookFunction
    {
        [FunctionName("BeaconWebhookFunction")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "BeaconWebhookFunction")]HttpRequestMessage req,
            [Queue("beacon-received-queue", Connection = "AzureWebJobsStorage")] IAsyncCollector<BeaconData> items,
            TraceWriter log,
            CancellationToken token)
        {
            var jsonContent = await req.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(jsonContent))
            {
                var beacons = JsonConvert.DeserializeObject<BeaconDataList>(jsonContent);
                if (beacons != null)
                {
                    foreach (var beacon in beacons.Beacons)
                    {
                        beacon.Identifier = GetBeaconIdentifier(beacon);

                        if (!IsAlreadyDiscovered(beacon))
                        {
                            SaveInLogTable(beacon);

                            await items.AddAsync(beacon, token);
                        }
                        else
                        {
                            LogHelper.Log(log, $"BeaconWebhookFunction called: beacon: {Newtonsoft.Json.JsonConvert.SerializeObject(beacon)}");
                        }
                    }
                }
            }
            LogHelper.Log(log, $"BeaconWebhookFunction called: {jsonContent}");

            await items.FlushAsync(token);
        }

        private static string GetBeaconIdentifier(BeaconData beacon)
        {
            var result = new StringBuilder();
            if (beacon.BeaconType.Equals("ibeacon", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append("1!");

                result.Append(beacon.IbeaconData.Uuid.ToString("N").ToLower())
                .Append(beacon.IbeaconData.MajorHex.ToLower())
                .Append(beacon.IbeaconData.MinorHex.ToLower());
            }
            else if (beacon.BeaconType.Equals("eddystone_uid", StringComparison.InvariantCultureIgnoreCase) && beacon.EddyStoneData != null)
            {
                result.Append("3!")
                .Append(beacon.EddyStoneData.NamespaceId.Replace("0x", ""))
                .Append(beacon.EddyStoneData.InstanceId.Replace("0x", ""));
            }
            return result.ToString();
        }

        private static bool IsAlreadyDiscovered(BeaconData beacon)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("BeaconLogTable");

            var query = new TableQuery<LocationStorage>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTimeOffset.Now.ToString("ddMMyyyy")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, beacon.Identifier)));

            var result = table.ExecuteQuery(query);

            return result.Any(x => DateTimeOffset.UtcNow.Subtract(x.Timestamp).Minutes < 5);
        }

        /// <summary>
        /// Save all beacons in a log table
        /// </summary>
        /// <param name="beacon"></param>
        private static void SaveInLogTable(BeaconData beacon)
        {
            var partitionKey = DateTime.Now.ToString("ddMMyyyy");

            var item = new BeaconDataStorage { PartitionKey = partitionKey, RowKey = beacon.Identifier, Timestamp = DateTimeOffset.Now, Data = Newtonsoft.Json.JsonConvert.SerializeObject(beacon) };

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("BeaconLogTable");

            var operation = TableOperation.InsertOrReplace(item);

            table.Execute(operation);
        }
    }
}
