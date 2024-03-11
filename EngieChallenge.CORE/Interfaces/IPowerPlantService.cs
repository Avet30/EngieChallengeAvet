using EngieChallenge.CORE.Domain;

namespace EngieChallenge.CORE.Interfaces
{
    public interface IPowerPlantService
    {
        List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad);

       
    }
}