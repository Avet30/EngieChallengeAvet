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

            var plannedOutputs = powerPlants
                .Select(p => new PlannedOutput
                {
                    PowerPlantName = p.Name,
                    PlantPower = 0.0M
                })
                .ToList();

            var sortedPlants = powerPlants
                .OrderBy(p => p.CalculatedFuelCost)
                .ThenByDescending(p => p.PMax)
                .ToList();

            List<PlannedOutput> bestPlan = null;
            decimal lowestCost = decimal.MaxValue;

            for (int i = 0; i < sortedPlants.Count; i++)
            {
                var testOutputs = plannedOutputs.Select(po => new PlannedOutput
                {
                    PowerPlantName = po.PowerPlantName,
                    PlantPower = 0.0M
                }).ToList();

                decimal testLoad = plannedLoad;

                for (int j = i; j < sortedPlants.Count; j++)
                {
                    var powerPlant = sortedPlants[j];
                    if (testLoad <= 0)
                        break;

                    decimal preliminaryPowerOutput;

                    if (powerPlant is WindTurbine)
                    {
                        preliminaryPowerOutput = powerPlant.CalculatedPMax;
                        if (testLoad >= preliminaryPowerOutput)
                        {
                            testOutputs = PlanLoad(testOutputs, testLoad, powerPlant, preliminaryPowerOutput);
                            testLoad -= preliminaryPowerOutput;
                        }
                        continue;
                    }

                    preliminaryPowerOutput = Math.Min(testLoad, powerPlant.CalculatedPMax);

                    if (preliminaryPowerOutput == testLoad && powerPlant.PMin < testLoad)
                    {
                        testOutputs = PlanLoad(testOutputs, testLoad, powerPlant, preliminaryPowerOutput);
                        testLoad -= preliminaryPowerOutput;
                        break;
                    }

                    preliminaryPowerOutput = AdjustForNextPlant(sortedPlants, j, testLoad, preliminaryPowerOutput, powerPlant);

                    if (preliminaryPowerOutput >= powerPlant.PMin)
                    {
                        testOutputs = PlanLoad(testOutputs, testLoad, powerPlant, preliminaryPowerOutput);
                        testLoad -= preliminaryPowerOutput;
                    }
                }

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

            if (bestPlan != null)
            {
                var sortedBestPlan = bestPlan
                    .OrderByDescending(po => po.PlantPower)
                    .ToList();

                return sortedBestPlan;
            }

            _logger.LogWarning($"Unable to fulfill planned load. Remaining load: {plannedLoad}");
            throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
        }

        private List<PlannedOutput> PlanLoad(List<PlannedOutput> plannedOutputs, decimal remainingLoad, PowerPlant powerPlant, decimal plannedPower)
        {
            var output = plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name);
            output.PlantPower = plannedPower;
            return plannedOutputs;
        }


        private List<PlannedOutput> HandleWindTurbine(List<PlannedOutput> plannedOutputs, decimal remainingLoad, PowerPlant powerPlant)
        {
            var windPowerOutput = powerPlant.CalculatedPMax;
            if (remainingLoad >= windPowerOutput)
            {
                plannedOutputs = PlanLoad(plannedOutputs, remainingLoad, powerPlant, windPowerOutput);
                remainingLoad -= windPowerOutput;
            }
            return plannedOutputs;
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

