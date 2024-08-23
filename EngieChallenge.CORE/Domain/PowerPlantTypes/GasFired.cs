namespace EngieChallenge.CORE.Domain.PowerPlantTypes;

public class GasFired : PowerPlant
{
    public override void ComputeEffectivePowerOutputAndFuelCost(Fuel fuel)
    {
        EffectivePowerOutput = PMax;
        FuelCostPerMWh = fuel.Gas / Efficiency;
    }
}