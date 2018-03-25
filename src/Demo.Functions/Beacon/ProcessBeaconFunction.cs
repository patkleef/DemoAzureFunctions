using System;
using System.Globalization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Demo.Functions.Completed;
using Demo.Functions.Google;
using Demo.Functions.Logging;
using Demo.Functions.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Refit;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace Demo.Functions.Beacon
{
    public static class ProcessBeaconFunction
    {
        private static string _googleApiKey => Environment.GetEnvironmentVariable("GoogleApiKey", EnvironmentVariableTarget.Process);
        private static readonly IGoogleService _googleService = RestService.For<IGoogleService>("https://maps.googleapis.com");

        [FunctionName("ProcessBeaconFunction")]
        [return: Queue("location-queue", Connection = "AzureWebJobsStorage")]
        public static async Task<LocationInfo> Run(
            [QueueTrigger("beacon-received-queue", Connection = "AzureWebJobsStorage")]BeaconData beacon,
            TraceWriter log, 
            ExecutionContext context)
        {
            var info = await GetInfo(log, beacon, context);
            if (info != null && !IsAlreadyDiscovered(info))
            {
                info.Beacon = beacon;
                info.Address = await GetAddress(info.Coordinates);
                info.Date = DateTime.UtcNow;
                
                LogHelper.Log(log, $"ProcessBeaconFunction called: beacon: {Newtonsoft.Json.JsonConvert.SerializeObject(info)}");

                return info;
            }
            LogHelper.Log(log, "ProcessBeaconFunction called: unkown beacon");

            return null;
        }

        private static async Task<LocationInfo> GetInfo(TraceWriter log, BeaconData beacon, ExecutionContext context)
        {
            var identifier = GetBeaconIdentifier(beacon);

            LogHelper.Log(log, $"GetBeaconIdentifier called: {identifier}");

            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }
            var path = $"{context.FunctionAppDirectory}\\beacons.csv";

            using (TextReader reader = File.OpenText(path))
            {
                var configuration = new Configuration { HasHeaderRecord = true, };

                var doubleConverterOptions = new TypeConverterOptions { CultureInfo = CultureInfo.InvariantCulture};

                var csv = new CsvReader(reader, configuration);
                csv.Configuration.TypeConverterOptionsCache.AddOptions<double>(doubleConverterOptions);
                
                await csv.ReadAsync(); // skip first header line

                while (await csv.ReadAsync())
                {
                    var name = csv.GetField<string>(0);
                    if (!string.IsNullOrEmpty(name) && name.Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new LocationInfo
                        {
                            Name = csv.GetField<string>(1),
                            Coordinates = new GeoCoordinates(csv.GetField<double>(4), csv.GetField<double>(5))
                        };
                    }
                }
            }
            return null;
        }

        private static string GetBeaconIdentifier(BeaconData beacon)
        {
            var result = new StringBuilder();
            if (beacon.BeaconType.Equals("ibeacon", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append("1!");
            }
            else if (beacon.BeaconType.Equals("eddystone", StringComparison.InvariantCultureIgnoreCase))
            {
                result.Append("3!");
            }
            result.Append(beacon.Data.Uuid.ToString("N").ToLower())
                .Append(beacon.Data.MajorHex.ToLower())
                .Append(beacon.Data.MinorHex.ToLower());
                

            return result.ToString();
        }

        private static async Task<string> GetAddress(GeoCoordinates coordinates)
        {
            var results = await _googleService.ReverseGeocoding(coordinates.ToString(), _googleApiKey);

            return results.results.FirstOrDefault()?.formatted_address;
        }

        private static bool IsAlreadyDiscovered(LocationInfo locationInfo)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));

            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference("Locations");

            var query = new TableQuery<LocationStorage>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, DateTimeOffset.Now.ToString("ddMMyyyy")),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, locationInfo.Name)));

            var result = table.ExecuteQuery(query);

            return result.Any(x => DateTimeOffset.Now.Subtract(x.Timestamp).Minutes > 15);
        }
    }
}
