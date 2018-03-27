using System;
using System.Globalization;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Demo.Functions.Google;
using Demo.Functions.Logging;
using Demo.Functions.Models;
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
            if (info != null)
            {
                info.Address = await GetAddress(info.Coordinates);
                info.Date = DateTime.UtcNow;

                LogHelper.Log(log, $"ProcessBeaconFunction called: beacon: {Newtonsoft.Json.JsonConvert.SerializeObject(info)}");

                return info;
            }
            else
            {
                LogHelper.Log(log, "ProcessBeaconFunction called: unkown beacon");
            }
            return null;
        }

        private static async Task<LocationInfo> GetInfo(TraceWriter log, BeaconData beacon, ExecutionContext context)
        {
            LogHelper.Log(log, $"GetBeaconIdentifier called: {beacon.Identifier}");

            if (string.IsNullOrEmpty(beacon.Identifier))
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
                    if (!string.IsNullOrEmpty(name) && name.Equals(beacon.Identifier, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new LocationInfo
                        {
                            Name = csv.GetField<string>(1),
                            Coordinates = new GeoCoordinates(csv.GetField<double>(4), csv.GetField<double>(5)),
                            Beacon = beacon
                        };
                    }
                }
            }
            return null;
        }

        private static async Task<string> GetAddress(GeoCoordinates coordinates)
        {
            var results = await _googleService.ReverseGeocoding(coordinates.ToString(), _googleApiKey);

            return results.results.FirstOrDefault()?.formatted_address;
        }
    }
}
