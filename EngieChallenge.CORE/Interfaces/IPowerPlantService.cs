using EngieChallenge.CORE.Models;

namespace EngieChallenge.CORE.Interfaces
{
    public interface IPowerPlantService
    {
        List<PlannedOutput> GetPlannedOutput(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad);
    }
}