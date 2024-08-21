using EngieChallenge.CORE.Domain;

public class WindTurbine : PowerPlant
{
    public override void ComputePMax(Fuel fuel)
    {
        CalculatedPMax = PMax * (fuel.Wind / 100.0M);
    }
    public override void ComputeFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = 0.0M;
    }
}

