using EngieChallenge.CORE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Domain
{
    public class PowerPlant
    {
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PowerPlantType Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
        public decimal CalculatedPMax { get; private set; }
        public decimal CalculatedFuelCost { get; private set; }

        public void CalculatePMax(Fuel fuel)
        {
            if (Type == PowerPlantType.windturbine)
            {
                CalculatedPMax = PMax / 100.0M * fuel.Wind;
            }
            else
            {
                CalculatedPMax = PMax;
            }
        }

        public void CalculateFuelCost(Fuel fuel)
        {
            if (Type == PowerPlantType.windturbine)
            {
                CalculatedFuelCost = 0.0M;
            }
            else if (Type == PowerPlantType.gasfired)
            {
                CalculatedFuelCost = fuel.Gas / Efficiency;
            }
            else // Assuming default is Turbojet
            {
                CalculatedFuelCost = fuel.Kerosine / Efficiency;
            }
        }
    }
}
