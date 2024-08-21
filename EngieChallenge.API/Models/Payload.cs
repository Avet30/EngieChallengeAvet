using EngieChallenge.CORE.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallenge.API.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        [Required]
        public Fuel Fuels { get; set; }
        [Required]
        public PowerPlant[] Powerplants { get; set; }
    }
}


