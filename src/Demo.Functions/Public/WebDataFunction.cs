using System.Linq;
using System.Net;
using System.Net.Http;
using Demo.Functions.Completed;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using Demo.Functions.Models;

namespace Demo.Functions.Public
{
    public static class WebDataFunction
    {
        [FunctionName("WebDataFunction")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            TraceWriter log)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("Locations");

            var query = new TableQuery<LocationStorage>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    DateTimeOffset.Now.ToString("ddMMyyyy")));

            var result = table.ExecuteQuery(query).ToList();

            if (!result.Any())
            {
                query = new TableQuery<LocationStorage>();

                result = table.ExecuteQuery(query)
                    .GroupBy(x => x.Name).Select(x => x.First()).ToList();
            }
            var response = req.CreateResponse(HttpStatusCode.OK,
                result.Select(x => Newtonsoft.Json.JsonConvert.DeserializeObject<LocationInfo>(x.Data)).OrderByDescending(x => x.Date).ToList());

            return response;
        }
    }
}
