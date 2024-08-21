using EngieChallenge.CORE.Domain;


public abstract class PowerPlant
{
    public string? Name { get; set; }
    public decimal Efficiency { get; set; }
    public decimal PMin { get; set; }
    public decimal PMax { get; set; }
    public decimal CalculatedPMax { get; protected set; }
    public decimal CalculatedFuelCost { get; protected set; }
    public abstract void ComputePMax(Fuel fuel);
    public abstract void ComputeFuelCost(Fuel fuel);

    public void ComputeValues(Fuel fuel)
    {
        ComputePMax(fuel);
        ComputeFuelCost(fuel);
    }

    public static void ComputeAllValues(IEnumerable<PowerPlant> plants, Fuel fuel)
    {
        foreach (var plant in plants)
        {
            plant.ComputeValues(fuel);
        }
    }
}

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

