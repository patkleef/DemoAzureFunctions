using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Demo.Functions.Logging;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
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
                        await items.AddAsync(beacon, token);
                    }
                }
            }
            LogHelper.Log(log, $"BeaconWebhookFunction called: {jsonContent}");

             await items.FlushAsync(token);
        }
    }
}
