using EngieChallenge.CORE.Domain;


public abstract class PowerPlant
{
    public string? Name { get; set; }
    public decimal Efficiency { get; set; }
    public decimal PMin { get; set; }
    public decimal PMax { get; set; }
    public decimal CalculatedPMax { get; protected set; }
    public decimal CalculatedFuelCost { get; protected set; }
    public abstract void CalculatePMax(Fuel fuel);
    public abstract void CalculateFuelCost(Fuel fuel);
}

public class WindTurbine : PowerPlant
{
    public override void CalculatePMax(Fuel fuel)
    {
        CalculatedPMax = PMax * (fuel.Wind / 100.0M);
    }
    public override void CalculateFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = 0.0M;
    }
}

public class GasFired : PowerPlant
{
    public override void CalculatePMax(Fuel fuel)
    {
        CalculatedPMax = PMax;
    }
    public override void CalculateFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = fuel.Gas / Efficiency;
    }
}

public class TurboJet : PowerPlant
{
    public override void CalculatePMax(Fuel fuel)
    {
        CalculatedPMax = PMax;
    }
    public override void CalculateFuelCost(Fuel fuel)
    {
        CalculatedFuelCost = fuel.Kerosine / Efficiency;
    }
}  

