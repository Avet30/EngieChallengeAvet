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
}

