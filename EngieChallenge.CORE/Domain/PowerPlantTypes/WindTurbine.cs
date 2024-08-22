namespace EngieChallenge.CORE.Domain.PowerPlantTypes;

public class WindTurbine : PowerPlant
{
    public override void ComputePMaxAndFuelCost(Fuel fuel)
    {
        EffectivePowerOutput = PMax * (fuel.Wind / 100.0M);
        FuelCostPerMWh = 0.0M; // No fuel cost for wind turbines
    }
}