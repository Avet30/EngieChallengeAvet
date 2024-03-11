using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Domain
{
    public class Fuel
    {
        [JsonPropertyName(@"gas(euro/MWh)")]
        public decimal Gas { get; set; }

        [JsonPropertyName(@"kerosine(euro/MWh)")]
        public decimal Kerosine { get; set; }

        [JsonPropertyName(@"wind(%)")]
        public decimal Wind { get; set; }
    }
}
