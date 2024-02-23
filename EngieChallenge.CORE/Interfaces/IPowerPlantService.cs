using EngieChallenge.CORE.Models;

namespace EngieChallenge.CORE.Interfaces
{
    public interface IPowerPlantService
    {
        List<PowerPlant> CalculateRealCostAndPower(List<PowerPlant> powerPlants, Fuel fuel);

        List<PowerPlant> OrderPowerPlants(List<PowerPlant> powerPlants, Fuel fuel);

        List<PlannedOutput> GetPlannedOutput(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad);
    }
}