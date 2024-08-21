using EngieChallenge.CORE.Domain;

public class TurboJet : PowerPlant
{
    public override void ComputePMax(Fuel fuel)
    {
        CalculatedPMax = PMax;
    }
    public override void ComputeFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = fuel.Kerosine / Efficiency;
    }
}  

