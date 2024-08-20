using EngieChallenge.CORE.Domain;
using EngieChallenge.CORE.Domain.Enums;
using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // Sort calculatedPlants by CalculatedFuelCost, but prioritize WindTurbine due to zero cost
            var sortedPlants = calculatedPlants
                .OrderBy(p => p.CalculatedFuelCost)
                .ThenByDescending(p => p is WindTurbine)
                .ToList();

            for (int i = 0; i < sortedPlants.Count; i++)
            {
                var powerPlant = sortedPlants[i];
                

                if (remainingLoad <= 0)
                    break;

                if (powerPlant is WindTurbine)
                {
                    // Wind turbines are either ON (producing CalculatedPMax) or OFF (producing 0)
                    var windPowerOutput = powerPlant.CalculatedPMax;

                    if (remainingLoad >= windPowerOutput)
                    {
                        PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, windPowerOutput);
                        usedPlants.Add(powerPlant.Name);
                    }
                    // If the remaining load cannot accommodate the full wind power output, we skip this wind turbine
                    continue;
                }

                // Plan preliminary load
                var preliminaryPowerOutput = Math.Min(remainingLoad, powerPlant.CalculatedPMax);

                
                if(preliminaryPowerOutput == remainingLoad && powerPlant.PMin < remainingLoad)
                {
                    PlanLoad(plannedOutputs, ref remainingLoad, powerPlant, preliminaryPowerOutput);
                    usedPlants.Add(powerPlant.Name);
                    break;
                }

                if (i < sortedPlants.Count - 1)
                {
                    var nextPowerPlant = sortedPlants[i + 1];
                    if (nextPowerPlant.PMin > (remainingLoad - preliminaryPowerOutput))
                    {
                        preliminaryPowerOutput = remainingLoad - nextPowerPlant.PMin;
                        if (preliminaryPowerOutput < powerPlant.PMin)
                        {
                            preliminaryPowerOutput = powerPlant.PMin;
                        }
                    }
                }

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
    }
}




//using EngieChallenge.CORE.Domain;
//using EngieChallenge.CORE.Domain.Enums;
//using EngieChallenge.CORE.Exceptions;
//using EngieChallenge.CORE.Interfaces;
//using Microsoft.Extensions.Logging;

//namespace EngieChallenge.CORE.Services
//{
//    public class PowerPlantService : IPowerPlantService
//    {
//        private readonly ILogger<PowerPlantService> _Logger;
//        public PowerPlantService(ILogger<PowerPlantService> logger)
//        {
//            _Logger = logger;
//        }
//        public List<PowerPlant> CalculateRealCostAndPower(List<PowerPlant> powerPlants, Fuel fuel)
//        {
//            try
//            {
//                foreach (var plant in powerPlants)
//                {
//                    plant.CalculatePMax(fuel);
//                    plant.CalculateFuelCost(fuel);
//                }
//            }
//            catch (Exception ex)
//            {
//                _Logger.LogError($"An error occurred while calculating real cost and power: {ex}");
//                throw ex;
//            }
//            return powerPlants;
//        }
//        private static decimal PlanLoadWithDifferential(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant, decimal differential)
//        {
//            decimal plannedPower = powerPlant.CalculatedPMax + differential;
//            remainingLoad -= plannedPower;
//            plannedOutputs.Add(new PlannedOutput { PowerPlantName = powerPlant.Name, PlantPower = plannedPower });
//            return plannedPower;
//        }

//        private static decimal PlanLoadWithRemaining(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant)
//        {
//            decimal plannedPower = remainingLoad;
//            plannedOutputs.Add(new PlannedOutput { PowerPlantName = powerPlant.Name, PlantPower = plannedPower });
//            remainingLoad -= plannedPower;
//            return plannedPower;
//        }

//        private static decimal PlanLoad(List<PlannedOutput> plannedOutputs, ref decimal remainingLoad, PowerPlant powerPlant)
//        {
//            decimal plannedPower = Math.Min(remainingLoad, powerPlant.CalculatedPMax);
//            plannedOutputs.Add(new PlannedOutput { PowerPlantName = powerPlant.Name, PlantPower = plannedPower });
//            remainingLoad -= plannedPower;
//            return plannedPower;
//        }

//        private static decimal CalculatePowerOutputs(List<PlannedOutput> plannedOutputs, decimal remainingLoad, List<PowerPlant> sortedPlants, int i, PowerPlant powerPlant, out bool shouldContinue)
//        {
//            shouldContinue = false;

//            if (powerPlant.CalculatedPMax >= 0 && powerPlant.PMin <= remainingLoad)
//            {
//                if (powerPlant.Type == PowerPlantType.windturbine && powerPlant.CalculatedPMax > remainingLoad)
//                {
//                    shouldContinue = true;
//                    return remainingLoad;
//                }

//                PowerPlant nextPlant = null;
//                if (i + 1 < sortedPlants.Count)
//                {
//                    nextPlant = sortedPlants[i + 1];
//                }

//                if (nextPlant != null && nextPlant.PMin <= remainingLoad)
//                {
//                    var differential = remainingLoad - (powerPlant.CalculatedPMax + nextPlant.PMin);

//                    if (differential < 0)
//                    {
//                        if (powerPlant.PMin <= remainingLoad && remainingLoad <= powerPlant.CalculatedPMax)
//                        {
//                            PlanLoadWithRemaining(plannedOutputs, ref remainingLoad, powerPlant);

//                            if (remainingLoad == 0)
//                            {
//                                shouldContinue = true;
//                                return remainingLoad;
//                            }
//                        }
//                        else if (powerPlant.CalculatedPMax <= remainingLoad && remainingLoad <= nextPlant.PMin)
//                        {
//                            if (nextPlant.PMin < remainingLoad - powerPlant.CalculatedPMax)
//                            {
//                                if (remainingLoad - powerPlant.CalculatedPMax > 0)
//                                {
//                                    PlanLoad(plannedOutputs, ref remainingLoad, powerPlant);
//                                }
//                                else
//                                {
//                                    PlanLoadWithDifferential(plannedOutputs, ref remainingLoad, powerPlant, differential);
//                                }
//                            }
//                            else
//                            {
//                                PlanLoad(plannedOutputs, ref remainingLoad, powerPlant);
//                            }
//                        }
//                        else if (remainingLoad - nextPlant.PMin > (remainingLoad - powerPlant.CalculatedPMax + nextPlant.PMin))
//                        {
//                            PlanLoadWithDifferential(plannedOutputs, ref remainingLoad, powerPlant, differential);
//                        }
//                    }
//                    else
//                    {
//                        PlanLoad(plannedOutputs, ref remainingLoad, powerPlant);
//                    }
//                }
//                else
//                {
//                    if (powerPlant.PMin <= remainingLoad && remainingLoad <= powerPlant.CalculatedPMax)
//                    {
//                        PlanLoadWithRemaining(plannedOutputs, ref remainingLoad, powerPlant);

//                        if (remainingLoad == 0)
//                        {
//                            shouldContinue = true;
//                            return remainingLoad;
//                        }
//                    }
//                    else
//                    {
//                        PlanLoad(plannedOutputs, ref remainingLoad, powerPlant);
//                    }
//                }
//            }
//            return remainingLoad;
//        }
//        public List<PlannedOutput> GetProductionPlan(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
//        {
//            var calculatedPlants = CalculateRealCostAndPower(powerPlants, fuel);
//            try
//            {
//                var plannedOutputs = new List<PlannedOutput>();
//                var temporaryOutputs = new List<PlannedOutput>();



//                var remainingLoad = plannedLoad;

//                // Sort calculatedPlants by CalculatedFuelCost
//                var sortedPlants = calculatedPlants.OrderBy(p => p.CalculatedFuelCost).ToList();

//                foreach(var plant in sortedPlants)
//                {
//                    if(plant.CalculatedPMax == 0)
//                    {

//                    }
//                }

//                for (int i = 0; i < sortedPlants.Count; i++)
//                {
//                    var powerPlant = sortedPlants[i];

//                    if (remainingLoad == 0)
//                        break; // load OK

//                    bool shouldContinue;
//                    remainingLoad = CalculatePowerOutputs(plannedOutputs, remainingLoad, sortedPlants, i, powerPlant, out shouldContinue);

//                    if (shouldContinue)
//                        continue;
//                }


//                if (remainingLoad > 0)
//                {
//                    _Logger.LogWarning($"Unable to fulfill planned load. Remaining load: {remainingLoad}");
//                    throw new PlannedOutputCalculationException("Unable to calculate planned output. Remaining load cannot be fulfilled.");
//                }
//                return plannedOutputs;
//            }
//            catch (Exception ex)
//            {
//                _Logger.LogError($"An error occurred while calculating planned output: {ex}");
//                throw new PlannedOutputCalculationException("Unable to calculate planned output. An error occurred.");
//            }
//        }
//    }
//}





