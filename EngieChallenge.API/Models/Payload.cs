using EngieChallenge.CORE.Domain;

namespace EngieChallenge.API.Models;

public class Payload
{
    public decimal Load { get; set; }
    public Fuel? Fuels { get; set; }
    public List<PowerPlant>? Powerplants { get; set; }
}


