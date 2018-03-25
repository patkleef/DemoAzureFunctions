using System;
using Demo.Functions.Logging;
using Demo.Functions.Models;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Demo.Functions.Completed
{
    public static class CompletedFunction
    {
        [FunctionName("CompletedFunction")]
        public static void Run([QueueTrigger("completed-queue", Connection = "AzureWebJobsStorage")]LocationInfo locationInfo, TraceWriter log)
        {
            var partitionKey = DateTime.Now.ToString("ddMMyyyy");
            
            var item = new LocationStorage { PartitionKey = partitionKey, RowKey = locationInfo.Beacon.BeaconAddress, Timestamp = DateTimeOffset.Now, Name = locationInfo.Name, Data = Newtonsoft.Json.JsonConvert.SerializeObject(locationInfo) };
            
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
            
            var tableClient = storageAccount.CreateCloudTableClient();
            
            var table = tableClient.GetTableReference("Locations");

            var operation = TableOperation.InsertOrReplace(item);

            table.Execute(operation);

            LogHelper.Log(log, $"CompletedFunction called: {item}");
        }
    }
}
