using EngieChallenge.CORE.Domain;
using EngieChallenge.CORE.Domain.Exceptions;
using EngieChallenge.CORE.Domain.PowerPlantTypes;
using EngieChallenge.CORE.Interfaces;
using Microsoft.Extensions.Logging;

namespace EngieChallenge.CORE.Services;

public class PowerPlantService : IPowerPlantService
{
    private readonly ILogger<PowerPlantService> _logger;

    public PowerPlantService(ILogger<PowerPlantService> logger)
    {
        _logger = logger;
    }

    public List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
    {
        powerPlants.ForEach(plant => plant.ComputePMaxAndFuelCost(fuel));

        //Sort plants
        var sortedPlants = powerPlants
            .OrderBy(p => p.FuelCostPerMWh)
            .ThenByDescending(p => p.PMax)
            .ToList();

        //Generate all plant results
        var scenarios = sortedPlants
            .Select(p => CreatePowerPlanScenario(sortedPlants, plannedLoad, p))
            .ToList();
        scenarios.Add(CreatePowerPlanScenario(sortedPlants, plannedLoad, null)); // Calculate without ignoring any plant

        //Find best result
        var bestScenario = scenarios
            .Where(r => r.RemainingLoad <= 0) // Ensure only valid results are considered
            .OrderBy(r => r.TotalCost)        // Find the one with the lowest cost
            .FirstOrDefault();

        if (bestScenario == null)
        {
            // Log and throw an exception if no valid plan was found
            _logger.LogWarning($"Unable to fulfill planned load. Remaining load: {plannedLoad}");
            throw new PlannedOutputCalculationException("Unable to calculate planned output. Demanded load cannot be fulfilled.");
        }
        // Initialize plannedOutputs with all power plants, including those with 0 power
        var plannedOutputs = sortedPlants
            .Select(p => new PlannedOutput
            {
                PowerPlantName = p.Name,
                PlantPower = bestScenario.PlannedOutputs?.FirstOrDefault(po => po.PowerPlantName == p.Name)?.PlantPower ?? 0.0M
            })
            .OrderByDescending(po => po.PlantPower)
            .ToList();

        return plannedOutputs;
    }

    private static PlanScenario CreatePowerPlanScenario(List<PowerPlant> sortedPlants, decimal plannedLoad, PowerPlant excludedPlant)
    {
        // Initialize the plannedOutputs list with all power plants, setting initial power to 0 for each.
        var plannedOutputs = sortedPlants
                    .Select(p => new PlannedOutput { PowerPlantName = p.Name, PlantPower = 0.0M })
                    .ToList();

        decimal remainingLoad = plannedLoad;

        foreach (var powerPlant in sortedPlants)
        {
            // Skip the power plant if it is the one we intend to ignore for this scenario.
            if (powerPlant == excludedPlant)
                continue;

            if (remainingLoad <= 0)
                break;

            decimal PowerOutput;

            // Special handling for wind turbines, as they have a fixed output based on wind conditions.
            if (powerPlant is WindTurbine)
            {
                PowerOutput = powerPlant.EffectivePowerOutput;

                // If the remaining load is greater than or equal to the turbine's maximum output,
                // assign the output and reduce the remaining load accordingly.
                if (remainingLoad >= PowerOutput)
                {
                    plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name).PlantPower = PowerOutput;
                    remainingLoad -= PowerOutput;
                }
                continue;
            }

            // For other types of power plants, determine the power output based on remaining load and the plant's capacity.
            PowerOutput = Math.Min(remainingLoad, powerPlant.EffectivePowerOutput);

            // If the preliminary power output exactly matches the remaining load and the plant's minimum output is less than the remaining load,
            // assign the output and break out of the loop since the load is fulfilled.
            if (PowerOutput == remainingLoad && powerPlant.PMin < remainingLoad)
            {
                plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name).PlantPower = PowerOutput;
                remainingLoad -= PowerOutput;
                break;
            }

            // Adjust the power output considering the next power plant's minimum power requirement.
            PowerOutput = AdjustForNextPlant(sortedPlants, sortedPlants.IndexOf(powerPlant), remainingLoad, PowerOutput, powerPlant);

            // If the adjusted power output is greater than or equal to the plant's minimum power, assign the output and reduce the remaining load.
            if (PowerOutput >= powerPlant.PMin)
            {
                plannedOutputs.First(po => po.PowerPlantName == powerPlant.Name).PlantPower = PowerOutput;
                remainingLoad -= PowerOutput;
            }
        }


        //Compute the total cost by summing each plant's output multiplied by its fuel cost per MWh.
        decimal totalCost = plannedOutputs.Sum(p => p.PlantPower * sortedPlants.First(pp => pp.Name == p.PowerPlantName).FuelCostPerMWh);

        return new PlanScenario
        {
            PlannedOutputs = plannedOutputs,
            TotalCost = totalCost,
            RemainingLoad = remainingLoad
        };
    }

    private static decimal AdjustForNextPlant(List<PowerPlant> sortedPlants, int index, decimal remainingLoad, decimal PowerOutput, PowerPlant powerPlant)
    {
        if (index >= sortedPlants.Count - 1) return PowerOutput;
        var nextPowerPlant = sortedPlants[index + 1];

        // Check if the next power plant's minimum output is greater than the remaining load after assigning the preliminary output
        if (nextPowerPlant.PMin <= (remainingLoad - PowerOutput)) return PowerOutput;

        // Adjust the preliminary output to ensure that the remaining load will be enough to meet the next plant's minimum output
        PowerOutput = remainingLoad - nextPowerPlant.PMin;

        // If the adjusted preliminary output is less than the current plant's minimum output, set it to the minimum output of the current plant
        if (PowerOutput < powerPlant.PMin)
        {
            PowerOutput = powerPlant.PMin;
        }
        return PowerOutput;
    }
}


