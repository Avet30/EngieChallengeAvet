using EngieChallenge.CORE.Domain;
using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using Microsoft.Extensions.Logging;

namespace EngieChallenge.CORE.Services
{
    public class PowerPlantService : IPowerPlantService
    {
        private readonly ILogger<PowerPlantService> _logger;

        public PowerPlantService(ILogger<PowerPlantService> logger)
        {
            _logger = logger;
        }

        public List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            PowerPlant.CalculateAllValues(powerPlants, fuel);

            // Initialize plannedOutputs with all power plants, defaulting power to 0.0
            var plannedOutputs = powerPlants
                .Select(p => new PlannedOutput
                {
                    PowerPlantName = p.Name,
                    PlantPower = 0.0M // Default power to 0.0
                })
                .ToList();

            var sortedPlants = powerPlants
                .OrderBy(p => p.CalculatedFuelCost)
                .ThenByDescending(p => p.PMax)
                .ToList();

            // Try all combinations
            List<PlannedOutput> bestPlan = null;
            decimal lowestCost = decimal.MaxValue;

            for (int i = 0; i < sortedPlants.Count; i++)
            {
                var testOutputs = plannedOutputs.Select(po => new PlannedOutput
                {
                    PowerPlantName = po.PowerPlantName,
                    PlantPower = 0.0M
                }).ToList();

                var testLoad = plannedLoad;

                for (int j = i; j < sortedPlants.Count; j++)
                {
                    var powerPlant = sortedPlants[j];
                    if (testLoad <= 0)
                        break;

                    decimal preliminaryPowerOutput;

                    // Handle wind turbines: they can only be ON or OFF
                    if (powerPlant is WindTurbine)
                    {
                        preliminaryPowerOutput = powerPlant.CalculatedPMax;
                        if (testLoad >= preliminaryPowerOutput)
                        {
                            PlanLoad(testOutputs, ref testLoad, powerPlant, preliminaryPowerOutput);
                        }
                        continue; // Move to the next plant
                    }

                    // Calculate preliminary load for non-wind turbines
                    preliminaryPowerOutput = Math.Min(testLoad, powerPlant.CalculatedPMax);

                    // Skip combinations that don't meet minimum load requirements of other plants
                    if (preliminaryPowerOutput == testLoad && powerPlant.PMin < testLoad)
                    {
                        PlanLoad(testOutputs, ref testLoad, powerPlant, preliminaryPowerOutput);
                        break;
                    }

                    preliminaryPowerOutput = AdjustForNextPlant(sortedPlants, j, testLoad, preliminaryPowerOutput, powerPlant);

                    if (preliminaryPowerOutput >= powerPlant.PMin)
                    {
                        PlanLoad(testOutputs, ref testLoad, powerPlant, preliminaryPowerOutput);
                    }
                }

                // Check if this combination meets the load and is cheaper
                if (testLoad <= 0)
                {
                    decimal totalCost = testOutputs.Sum(p => p.PlantPower * powerPlants.First(pp => pp.Name == p.PowerPlantName).CalculatedFuelCost);

                    if (totalCost < lowestCost)
                    {
                        bestPlan = testOutputs;
                        lowestCost = totalCost;
                    }
                }
            }

            // If a valid plan is found, return it
            if (bestPlan != null)
            {
                return bestPlan;
            }

            // If no valid plan was found, throw an exception
            _logger.LogWarning($"Unable to fulfill planned load. Remaining load: {plannedLoad}");
            throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
        }

        private void PlanLoad(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant, decimal plannedPower)
        {
            var output = plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name);
            output.PlantPower = plannedPower;
            remainingLoad -= plannedPower;
        }

        private void HandleWindTurbine(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant, HashSet<string> usedPlants)
        {
            var windPowerOutput = powerPlant.CalculatedPMax;
            if (remainingLoad >= windPowerOutput)
            {
                PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, windPowerOutput);
            }
        }

        private decimal AdjustForNextPlant(List<PowerPlant> sortedPlants, int index, decimal remainingLoad, decimal preliminaryPowerOutput, PowerPlant powerPlant)
        {
            if (index < sortedPlants.Count - 1)
            {
                var nextPowerPlant = sortedPlants[index + 1];
                if (nextPowerPlant.PMin > (remainingLoad - preliminaryPowerOutput))
                {
                    preliminaryPowerOutput = remainingLoad - nextPowerPlant.PMin;
                    if (preliminaryPowerOutput < powerPlant.PMin)
                    {
                        preliminaryPowerOutput = powerPlant.PMin;
                    }
                }
            }
            return preliminaryPowerOutput;
        }
    }
}

