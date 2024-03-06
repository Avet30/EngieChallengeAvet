using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using EngieChallenge.CORE.Models;
using EngieChallenge.CORE.Models.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EngieChallenge.CORE.Services
{
    public class PowerPlantService : IPowerPlantService
    {
        private readonly ILogger<PowerPlantService> _Logger;
        public PowerPlantService(ILogger<PowerPlantService> logger)
        {
            _Logger = logger;
        }
        public List<PowerPlant> CalculateRealCostAndPower(List<PowerPlant> powerPlants, Fuel fuel)
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
        public List<PowerPlant> OrderWindTurbines(List<PowerPlant> powerPlants, Fuel fuel)
        {
            var realCostAndPowerProducedByPlants = CalculateRealCostAndPower(powerPlants, fuel);

            try
            {
                var orderedPlants = realCostAndPowerProducedByPlants
                    //Ordered By Efficiency
                    .OrderByDescending(x => x.Efficiency)
                    //If same efficiency then ordered by realFuelCost
                    .ThenBy(x => x.CalculatedFuelCost)
                    //If same efficiency and realFuelCost then ordered by realPMax
                    .ThenByDescending(x => x.CalculatedPMax)
                    .ToList();
                return orderedPlants;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while ordering power plants: {ex}");
                throw ex;
            }
        }
        public List<PowerPlant> OrderGasFiredAndTurboJet(List<PowerPlant> powerPlants, Fuel fuel)
        {
            var realCostAndPowerProducedByPlants = CalculateRealCostAndPower(powerPlants, fuel);
            try
            {
                var orderedPlants = realCostAndPowerProducedByPlants
                    //Ordered By CalculatedFuelCost
                    .OrderBy(x => x.CalculatedFuelCost)
                    //If same CalculatedFuelCost then ordered by CalculatedPMax
                    .ThenBy(x => x.CalculatedPMax)
                    .ToList();
                return orderedPlants;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while ordering power plants: {ex}");
                throw ex;
            }
        }
        private decimal CalculateTotalNextWindPower(List<PowerPlant> orderedPlantsWindTurbines, PowerPlant windTurbine, decimal remainingLoad)
        {
            decimal totalNextWindPower = 0;
            bool foundCurrentWindTurbine = false;

            foreach (var plant in orderedPlantsWindTurbines)
            {
                if (!foundCurrentWindTurbine)
                {
                    if (plant == windTurbine)
                    {
                        foundCurrentWindTurbine = true;
                    }
                    continue; // Skip until the current windTurbine is found
                }

                if (plant.Type == PowerPlantType.windturbine && windTurbine.CalculatedPMax <= remainingLoad)
                {
                    totalNextWindPower += plant.CalculatedPMax;
                }
                else
                {
                    break; // Stop adding power if the condition is not met
                }
            }
            return totalNextWindPower;
        }
        private (decimal plannedPower, decimal updatedRemainingLoad) CalculatePlannedPower(List<PowerPlant> orderedPlantsGasAndTurboJet, int currentIndex, decimal remainingLoad)
        {
            var powerPlant = orderedPlantsGasAndTurboJet[currentIndex];
            decimal plannedPower = 0;
            decimal updatedRemainingLoad = remainingLoad; // Initialize updatedRemainingLoad with remainingLoad

            if (powerPlant.PMin <= remainingLoad && remainingLoad <= powerPlant.CalculatedPMax)
            {
                plannedPower = remainingLoad;
                updatedRemainingLoad = 0; // Objective of load OK
            }
            else if (remainingLoad > powerPlant.CalculatedPMax)
            {
                if (currentIndex + 1 < orderedPlantsGasAndTurboJet.Count)
                {
                    var nextPlant = orderedPlantsGasAndTurboJet[currentIndex + 1];
                    var differential = remainingLoad - (powerPlant.CalculatedPMax + nextPlant.PMin);

                    if (differential < 0)
                    {
                        plannedPower = (powerPlant.CalculatedPMax + differential);
                        updatedRemainingLoad -= plannedPower;
                        // If the remaining load is less than 0, use the current plant as it fulfills the role
                    }
                    else
                    {
                        plannedPower = powerPlant.CalculatedPMax;
                        updatedRemainingLoad -= powerPlant.CalculatedPMax;
                    }
                }
            }
            return (plannedPower, updatedRemainingLoad);
        }
        public List<PlannedOutput> GetPlannedOutput(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            var orderedPlantsWindTurbines = OrderWindTurbines(powerPlants, fuel);
            var orderedPlantsGasAndTurboJet = OrderGasFiredAndTurboJet(powerPlants, fuel);
            try
            {
                var plannedOutputs = new List<PlannedOutput>();
                var remainingLoad = plannedLoad;

                foreach (var windTurbine in orderedPlantsWindTurbines.Where(p => p.Type == PowerPlantType.windturbine && p.CalculatedPMax != 0))
                {
                    if (remainingLoad == 0)
                        break; // load OK

                    decimal windPower = windTurbine.CalculatedPMax; //is there sufficient wind to turn on WindTurbines ?
                    decimal totalNextWindPower = CalculateTotalNextWindPower(orderedPlantsWindTurbines, windTurbine, remainingLoad);

                    //If the cumulative power of the next wind turbines plus the current one does not exceed the remaining load,
                    //use the wind turbine at its calculated PMax.
                    if (windPower + totalNextWindPower <= remainingLoad && windPower >= windTurbine.PMin)
                    {
                        plannedOutputs.Add(new PlannedOutput { PowerPlantName = windTurbine.Name, PowerPlantPower = windPower });
                        remainingLoad -= windPower;
                    }
                }

                for (int i = 0; i < orderedPlantsGasAndTurboJet.Count; i++)
                {
                    var powerPlant = orderedPlantsGasAndTurboJet[i];

                    if (powerPlant.Type == PowerPlantType.windturbine)
                        continue;

                    if (remainingLoad == 0)
                        break; // Objective of load OK

                    //decimal plannedPower = CalculatePlannedPower(orderedPlantsGasAndTurboJet, i, remainingLoad);
                    (decimal plannedPower, decimal updatedRemainingLoad) = CalculatePlannedPower(orderedPlantsGasAndTurboJet, i, remainingLoad);

                    remainingLoad = updatedRemainingLoad; // Update remainingLoad with the updatedRemainingLoad from the method

                    if (plannedPower > 0)
                    {
                        plannedOutputs.Add(new PlannedOutput { PowerPlantName = powerPlant.Name, PowerPlantPower = plannedPower });
                    }
                }

                if (remainingLoad > 0) // If remaining load is still not <= O then Log Alert
                {
                    _Logger.LogWarning($"Unable to fulfill planned load. Remaining load: {remainingLoad}");
                    throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
                }
                return plannedOutputs;
            }
            catch (Exception ex)
            {
                // Log the exception
                _Logger.LogError($"An error occurred while calculating planned output: {ex}");
                throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
            }
        }
    }
}





