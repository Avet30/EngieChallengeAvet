namespace EngieChallenge.CORE.Domain.PowerPlantTypes;

public class TurboJet : PowerPlant
{
    public override void ComputePMaxAndFuelCost(Fuel fuel)
    {
        EffectivePowerOutput = PMax;
        FuelCostPerMWh = fuel.Kerosine / Efficiency;
    }
}