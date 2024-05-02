using EngieChallenge.CORE.Domain.Enums;
using EngieChallenge.CORE.Domain;
using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Services
{
    public class PowerPlantServiceBetter : IPowerPlantService
    {
        private readonly ILogger<PowerPlantServiceBetter> _Logger;
        public PowerPlantServiceBetter(ILogger<PowerPlantServiceBetter> logger)
        {
            _Logger = logger;
        }
        public List<PowerPlant> ComputeRealCostAndPower(List<PowerPlant> powerPlants, Fuel fuel)
        {
            try
            {
                foreach (var plant in powerPlants)
                {
                    plant.CalculatePMax(fuel);
                    plant.CalculateFuelCost(fuel);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while calculating real cost and power: {ex}");
                throw ex;
            }
            return powerPlants;
        }

        public List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            var calculatedPlants = ComputeRealCostAndPower(powerPlants, fuel);
            try
            {
                var plannedOutputs = new List<PlannedOutput>();
                var remainingLoad = plannedLoad;

                var sortedPlants = calculatedPlants.OrderBy(p => p.CalculatedFuelCost).ToList();


                for (int i = 0; i < sortedPlants.Count; i++)
                {
                    var powerPlant = sortedPlants[i];

                    if (remainingLoad == 0)
                        break; // load OK

                    if(powerPlant.PMin > remainingLoad || powerPlant.CalculatedPMax == 0)
                    {
                        continue;
                    }

                    var plannedPower = remainingLoad - powerPlant.CalculatedPMax;
                    remainingLoad = plannedPower;


                    plannedOutputs.Add(new PlannedOutput { PowerPlantName = powerPlant.Name, PlantPower = powerPlant.CalculatedPMax });


                    if (remainingLoad == 0)
                        break; // load OK
                }

                if (remainingLoad > 0)
                {
                    _Logger.LogWarning($"Unable to fulfill planned load. Remaining load: {remainingLoad}");
                    throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
                }
                return plannedOutputs;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while calculating planned output: {ex}");
                throw new PlannedOutputCalculationException("Unable to calculate planned output. An error occurred.");
            }
        }
    }
}
