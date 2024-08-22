namespace EngieChallenge.CORE.Domain;

public abstract class PowerPlant
{
    public string? Name { get; set; }
    public decimal Efficiency { get; set; }
    public decimal PMin { get; set; }
    public decimal PMax { get; set; }
    public decimal EffectivePowerOutput { get; protected set; }
    public decimal FuelCostPerMWh { get; protected set; }
    public abstract void ComputePMaxAndFuelCost(Fuel fuel);
}