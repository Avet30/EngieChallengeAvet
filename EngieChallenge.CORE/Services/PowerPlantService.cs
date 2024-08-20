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

        private List<PowerPlant> CalculateRealCostAndPower(List<PowerPlant> powerPlants, Fuel fuel)
        {
            powerPlants.ForEach(p =>
            {
                p.CalculatePMax(fuel);
                p.CalculateFuelCost(fuel);
            });

            return powerPlants;
        }

        private void PlanLoad(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant, decimal plannedPower)
        {
            plannedOutputs.Add(new PlannedOutput
            {
                PowerPlantName = powerPlant.Name,
                PlantPower = plannedPower
            });

            remainingLoad -= plannedPower;
        }

        public List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            var calculatedPlants = CalculateRealCostAndPower(powerPlants, fuel);
            var plannedOutputs = new List<PlannedOutput>();
            var remainingLoad = plannedLoad;
            var usedPlants = new HashSet<string>();

            // Priority on WindTurbines because they cost 0
            var sortedPlants = calculatedPlants
                .OrderBy(p => p.CalculatedFuelCost)
                .ThenByDescending(p => p is WindTurbine)
                .ToList();

            for (int i = 0; i < sortedPlants.Count; i++)
            {
                var powerPlant = sortedPlants[i];

                if (remainingLoad <= 0)
                    break;

                // Handle Wind Turbines first
                if (powerPlant is WindTurbine)
                {
                    HandleWindTurbine(plannedOutputs, ref remainingLoad, powerPlant, usedPlants);
                    continue;
                }

                // Calculate preliminary load
                var preliminaryPowerOutput = Math.Min(remainingLoad, powerPlant.CalculatedPMax);

                // If the remaining load matches the preliminary output and plant can handle it, exit early
                if (preliminaryPowerOutput == remainingLoad && powerPlant.PMin < remainingLoad)
                {
                    PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, preliminaryPowerOutput);
                    usedPlants.Add(powerPlant.Name);
                    break;
                }

                // Adjust preliminary power output if the next plant would be undersized
                preliminaryPowerOutput = AdjustForNextPlant(sortedPlants, i, remainingLoad, preliminaryPowerOutput, powerPlant);

                // Add the plant to the planned outputs if it meets the minimum output requirement
                if (preliminaryPowerOutput >= powerPlant.PMin)
                {
                    PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, preliminaryPowerOutput);
                    usedPlants.Add(powerPlant.Name);
                }
            }

            // Validate if remaining load can be distributed properly
            if (remainingLoad > 0)
            {
                // Verify if the remaining load fits into lowest cost suitable plant step-wise
                decimal remainder = remainingLoad;
                foreach (var powerPlant in sortedPlants)
                {
                    if (usedPlants.Contains(powerPlant.Name) &&
                        powerPlant.PMin <= remainder && remainder <= powerPlant.CalculatedPMax)
                    {
                        PlanLoad(plannedOutputs, ref remainder, powerPlant, remainder);
                        usedPlants.Add(powerPlant.Name);
                        remainingLoad = remainder;
                        break;
                    }
                }
            }

            // Error if unmet remaining load
            if (remainingLoad > 0)
            {
                _logger.LogWarning($"Unable to fulfill planned load. Remaining load: {remainingLoad}");
                throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
            }

            return plannedOutputs;
        }

        private void HandleWindTurbine(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant, HashSet<string> usedPlants)
        {
            var windPowerOutput = powerPlant.CalculatedPMax;
            if (remainingLoad >= windPowerOutput)
            {
                PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, windPowerOutput);
                usedPlants.Add(powerPlant.Name);
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

