using EngieChallenge.CORE.Domain;

public class GasFired : PowerPlant
{
    public override void ComputePMax(Fuel fuel)
    {
        CalculatedPMax = PMax;
    }
    public override void ComputeFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = fuel.Gas / Efficiency;
    }
}

