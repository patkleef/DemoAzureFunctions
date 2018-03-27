using Newtonsoft.Json;

namespace Demo.Functions.Beacon
{
    public class BeaconData
    {
        public string Identifier { get; set; }
        public string BeaconAddress { get; set; }
        public string BeaconType { get; set; }
        [JsonProperty("ibeaconData")]
        public IbeaconParameters IbeaconData { get; set; }

        [JsonProperty("eddystoneUidData")]
        public EddystoneParameters EddyStoneData { get; set; }

        public bool IsBlocked { get; set; }
        public long LastMinuteSeen { get; set; }
        public long LastSeen { get; set; }
        public decimal Distance { get; set; }
        public int Rssi { get; set; }
        public int Hashcode { get; set; }
        public int Manufacturer { get; set; }
        public int TxPower { get; set; }
    }
}
