using EngieChallenge.CORE.Domain;
using EngieChallenge.CORE.Domain.Exceptions;
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

        public List<PlannedOutput> GetProductionPlan(PowerPlant[] powerPlants, Fuel fuel, decimal plannedLoad)
        { 
            var computedPlants = ComputeRealCostAndPower(powerPlants, fuel);

            //Sort plants
            var sortedPlants = computedPlants
                .OrderBy(p => p.CalculatedFuelCost)
                .ThenByDescending(p => p.PMax)
                .ToList();

            //Generate all plant results
            var results = sortedPlants
                .Select(p => GeneratePlanResult(sortedPlants, plannedLoad, p))
                .ToList();
            results.Add(GeneratePlanResult(sortedPlants, plannedLoad, null)); // Calculate without ignoring any plant

            //Find best result
            var bestResult = results
                .Where(r => r.RemainingLoad <= 0) // Ensure only valid results are considered
                .OrderBy(r => r.TotalCost)        // Find the one with the lowest cost
                .FirstOrDefault();

            if (bestResult != null)
            {
                // Initialize plannedOutputs with all power plants, including those with 0 power
                var finalOutputs = sortedPlants
                    .Select(p => new PlannedOutput
                    {
                        PowerPlantName = p.Name,
                        PlantPower = bestResult.PlannedOutputs?.FirstOrDefault(po => po.PowerPlantName == p.Name)?.PlantPower ?? 0.0M
                    })
                    .OrderByDescending(po => po.PlantPower)
                    .ToList();

                return finalOutputs;
            }

            // If no valid plan was found, throw an exception
            _logger.LogWarning($"Unable to fulfill planned load. Remaining load: {plannedLoad}");
            throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
        }

        private PlanScenario GeneratePlanResult(List<PowerPlant> sortedPlants, decimal plannedLoad, PowerPlant elementToIgnore)
        {
            // Initialize the plannedOutputs list with all power plants, setting initial power to 0 for each.
            var plannedOutputs = sortedPlants
                .Select(p => new PlannedOutput
                {
                    PowerPlantName = p.Name,
                    PlantPower = 0.0M
                })
                .ToList();

            decimal remainingLoad = plannedLoad;

            foreach (var powerPlant in sortedPlants)
            {
                // Skip the power plant if it is the one we intend to ignore for this scenario.
                if (powerPlant == elementToIgnore)
                    continue;

                if (remainingLoad <= 0)
                    break;

                decimal preliminaryPowerOutput;

                // Special handling for wind turbines, as they have a fixed output based on wind conditions.
                if (powerPlant is WindTurbine)
                {
                    preliminaryPowerOutput = powerPlant.CalculatedPMax;

                    // If the remaining load is greater than or equal to the turbine's maximum output,
                    // assign the output and reduce the remaining load accordingly.
                    if (remainingLoad >= preliminaryPowerOutput)
                    {
                        PlanLoad(plannedOutputs, remainingLoad, powerPlant, preliminaryPowerOutput);
                        remainingLoad -= preliminaryPowerOutput;
                    }
                    continue;
                }

                // For other types of power plants, determine the power output based on remaining load and the plant's capacity.
                preliminaryPowerOutput = Math.Min(remainingLoad, powerPlant.CalculatedPMax);

                // If the preliminary power output exactly matches the remaining load and the plant's minimum output is less than the remaining load,
                // assign the output and break out of the loop since the load is fulfilled.
                if (preliminaryPowerOutput == remainingLoad && powerPlant.PMin < remainingLoad)
                {
                    PlanLoad(plannedOutputs, remainingLoad, powerPlant, preliminaryPowerOutput);
                    remainingLoad -= preliminaryPowerOutput;
                    break;
                }

                // Adjust the power output considering the next power plant's minimum power requirement.
                preliminaryPowerOutput = AdjustForNextPlant(sortedPlants, sortedPlants.IndexOf(powerPlant), remainingLoad, preliminaryPowerOutput, powerPlant);

                // If the adjusted power output is greater than or equal to the plant's minimum power, assign the output and reduce the remaining load.
                if (preliminaryPowerOutput >= powerPlant.PMin)
                {
                    PlanLoad(plannedOutputs, remainingLoad, powerPlant, preliminaryPowerOutput);
                    remainingLoad -= preliminaryPowerOutput;
                }
            }

            decimal totalCost = plannedOutputs.Sum(p => p.PlantPower * sortedPlants.First(pp => pp.Name == p.PowerPlantName).CalculatedFuelCost);

            return new PlanScenario
            {
                PlannedOutputs = plannedOutputs,
                TotalCost = totalCost,
                RemainingLoad = remainingLoad
            };
        }


        private List<PlannedOutput> PlanLoad(List<PlannedOutput> plannedOutputs, decimal remainingLoad, PowerPlant powerPlant, decimal plannedPower)
        {
            plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name).PlantPower = plannedPower;
            return plannedOutputs;
        }


        private decimal AdjustForNextPlant(List<PowerPlant> sortedPlants, int index, decimal remainingLoad, decimal preliminaryPowerOutput, PowerPlant powerPlant)
        {
            if (index < sortedPlants.Count - 1)
            {
                var nextPowerPlant = sortedPlants[index + 1];

                // Check if the next power plant's minimum output is greater than the remaining load after assigning the preliminary output
                if (nextPowerPlant.PMin > (remainingLoad - preliminaryPowerOutput))
                {
                    // Adjust the preliminary output to ensure that the remaining load will be enough to meet the next plant's minimum output
                    preliminaryPowerOutput = remainingLoad - nextPowerPlant.PMin;

                    // If the adjusted preliminary output is less than the current plant's minimum output, set it to the minimum output of the current plant
                    if (preliminaryPowerOutput < powerPlant.PMin)
                    {
                        preliminaryPowerOutput = powerPlant.PMin;
                    }
                }
            }

            return preliminaryPowerOutput;
        }


        private PowerPlant[] ComputeRealCostAndPower(PowerPlant[] powerPlants, Fuel fuel)
        {
            foreach (var plant in powerPlants)
            {
                plant.ComputePMax(fuel);
                plant.ComputeFuelCost(fuel);
            }
            return powerPlants;
        }
    }
}


