using EngieChallenge.CORE.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallenge.API.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        public Fuel Fuels { get; set; }
        public List<PowerPlant> Powerplants { get; set; }
    }
}


