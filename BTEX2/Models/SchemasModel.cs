
using Newtonsoft.Json;

namespace BitCWallet
{
    class SchemasModel
    {
        public class SchemaInsightBitPayCom
        {
            [JsonProperty(Required = Required.Always)]
            public string addrStr { get; set; }
            [JsonProperty(Required = Required.Always)]
            public double balance { get; set; }
            public double balanceSat { get; set; }
            public double totalReceived { get; set; }
            public double totalReceivedSat { get; set; }
            public int unconfirmedbalance { get; set; }
            public int unconfirmedTXapperances { get; set; }
            public int txAppearances { get; set; }
            public string[] transactions { get; set; }
        }
    }
}