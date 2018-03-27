using System;
using Demo.Functions.Logging;
using Demo.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace Demo.Functions.Completed
{
    public static class CompletedFunction
    {
        [FunctionName("CompletedFunction")]
        [return: Table("Locations")]
        public static LocationStorage Run([QueueTrigger("completed-queue", Connection = "AzureWebJobsStorage")]LocationInfo locationInfo, TraceWriter log)
        {
            var partitionKey = DateTime.Now.ToString("ddMMyyyy");

            var item = new LocationStorage { PartitionKey = partitionKey, RowKey = locationInfo.Beacon.Identifier, Timestamp = locationInfo.Date, Name = locationInfo.Name, Data = Newtonsoft.Json.JsonConvert.SerializeObject(locationInfo) };

            LogHelper.Log(log, $"CompletedFunction called: {item}");

            return item;
        }
    }
}
