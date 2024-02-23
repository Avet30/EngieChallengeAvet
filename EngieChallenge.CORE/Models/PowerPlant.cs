using EngieChallenge.CORE.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Models
{
    public class PowerPlant
    {
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PowerPlantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
        public decimal CalculatedPMax { get; set; }
        public decimal CalculatedFuelCost { get; set; }
    }
}
