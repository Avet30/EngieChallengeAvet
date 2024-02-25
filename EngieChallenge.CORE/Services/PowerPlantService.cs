using EngieChallenge.CORE.Interfaces;
using EngieChallenge.CORE.Models;
using EngieChallenge.CORE.Models.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            List<PowerPlant> pwPlants = new List<PowerPlant>();

            try
            {
                foreach (var plant in powerPlants)
                {
                    //Calcul des Coûts et Puissance réel par rapport puissance du vent "Windturbine"
                    if (plant.Type == PowerPlantType.windturbine)
                    {
                        plant.CalculatedPMax = plant.PMax / 100.0M * fuel.Wind;
                        plant.CalculatedFuelCost = 0.0M;
                    }
                    //Calcul des Coûts et Puissance réel par rapport à l'efficience "Gasfired"
                    else if (plant.Type == PowerPlantType.gasfired)
                    {
                        plant.CalculatedPMax = plant.PMax;
                        plant.CalculatedFuelCost = fuel.Gas / plant.Efficiency;
                    }
                    //Calcul des Coûts et Puissance réel par rapport à l'efficience "Turbojet"
                    else
                    {
                        plant.CalculatedPMax = plant.PMax;
                        plant.CalculatedFuelCost = fuel.Kerosine / plant.Efficiency;
                    }

                    pwPlants.Add(plant);
                }

            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while calculating real cost and power: {ex}");
                throw ex;
            }

            return pwPlants;
        }

        //public List<PowerPlant> OrderPowerPlants(List<PowerPlant> powerPlants, Fuel fuel)
        //{
        //    var realCostAndPowerProducedByPlants = CalculateRealCostAndPower(powerPlants, fuel);

        //    try
        //    {
        //        var orderedPlants = realCostAndPowerProducedByPlants
        //            //Ordonné par Efficiency
        //            .OrderByDescending(x => x.Efficiency)
        //            //Si même efficiency alors ordonné par le realFuelCost
        //            .ThenBy(x => x.CalculatedFuelCost)
        //            //Si même efficiency et realFuelCost alors ordonné par le realPMax
        //            .ThenByDescending(x => x.CalculatedPMax)
        //            .ToList();
        //        return orderedPlants;
        //    }
        //    catch (Exception ex)
        //    {
        //        _Logger.LogError($"An error occurred while ordering power plants: {ex}");
        //        throw ex;
        //    }
        //}

        public List<PlannedOutput> GetPlannedOutput(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            //var orderedPlants = OrderPowerPlants(powerPlants, fuel);
            var orderedPlants = CalculateRealCostAndPower(powerPlants, fuel);

            try
            {
                var plannedOutputs = new List<PlannedOutput>();
                var remainingLoad = plannedLoad;
 
                foreach (var windTurbine in orderedPlants.Where(p => p.Type == PowerPlantType.windturbine))
                {
                    if (remainingLoad == 0)
                        break; // Objectif de load OK

                    // Y a t'il du vent pour utiliser la PmaxCalculé des windturbines ?
                    decimal windPower = windTurbine.CalculatedPMax;

                    decimal totalNextWindPower = 0;
                    bool foundCurrentWindTurbine = false;

                    foreach (var plant in orderedPlants)
                    {
                        if (!foundCurrentWindTurbine)
                        {
                            if (plant == windTurbine)
                            {
                                foundCurrentWindTurbine = true;
                            }
                            continue; // Skip jusqu'à trouver le windturbine actuel
                        }

                        if (plant.Type == PowerPlantType.windturbine && windTurbine.CalculatedPMax <= remainingLoad)
                        {
                            totalNextWindPower += plant.CalculatedPMax;
                        }
                        else
                        {
                            break; //On arrête de rajouter du Power si la condition n'est pas atteinte
                        }
                    }

                    // Si la puissance cumulé des prochains wind turbines + l'actuel ne dépasse pas le Remaining Load
                    // Utiliser le windturbine à sa PMax Calculé
                    if (windPower + totalNextWindPower <= remainingLoad && windPower >= windTurbine.PMin)
                    {
                        plannedOutputs.Add(new PlannedOutput { Name = windTurbine.Name, P = windPower });
                        remainingLoad -= windPower;
                    }
                }

                //Après tri des windturbines, si load restant > 0 , alors on utilise les autres type de Plants.
                foreach (var powerPlant in orderedPlants.Where(p => p.Type != PowerPlantType.windturbine))
                {
                    if (remainingLoad == 0)
                        break; // Objectif de load OK

                    decimal plannedPower = 0;

                    if (powerPlant.PMin <= remainingLoad && remainingLoad <= powerPlant.CalculatedPMax)
                    {
                        plannedPower = remainingLoad;
                        remainingLoad = 0; //Objectif de load OK
                    }
                    else if (remainingLoad > powerPlant.CalculatedPMax)
                    {
                        plannedPower = powerPlant.CalculatedPMax;
                        remainingLoad -= powerPlant.CalculatedPMax;
                    }

                    if (plannedPower > 0)
                    {
                        plannedOutputs.Add(new PlannedOutput { Name = powerPlant.Name, P = plannedPower });
                    }
                }

                // Si il reste du load et que les plants ne suffisent pas, logger Alerte
                if (remainingLoad > 0)
                {
                    _Logger.LogWarning($"Unable to fulfill planned load. Remaining load: {remainingLoad}");
                }

                return plannedOutputs;
            }
            catch (Exception ex)
            {
                // Log the exception
                _Logger.LogError($"An error occurred while calculating planned output: {ex}");
                throw;
            }
        }
    }
}
