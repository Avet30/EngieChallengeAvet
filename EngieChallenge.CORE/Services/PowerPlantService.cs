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

                //logger pour chaque plant
                foreach (var plant in pwPlants)
                {
                    _Logger.LogInformation($"Plant: {plant.Name}, Type: {plant.Type}, CalculatedPMax: {plant.CalculatedPMax}, CalculatedFuelCost: {plant.CalculatedFuelCost}");
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while calculating real cost and power: {ex}");
                throw ex;
            }

            return pwPlants;
        }

        public List<PowerPlant> OrderPowerPlants(List<PowerPlant> powerPlants, Fuel fuel)
        {
            var realCostAndPowerProducedByPlants = CalculateRealCostAndPower(powerPlants, fuel);

            try
            {
                var orderedPlants = powerPlants
                    //Ordonné par Efficiency
                    .OrderByDescending(x => x.Efficiency)
                    //Si même efficiency alors ordonné par le realFuelCost
                    .ThenBy(x => x.CalculatedFuelCost)
                    //Si même efficiency et realFuelCost alors ordonné par le realPMax
                    .ThenByDescending(x => x.CalculatedPMax)
                    .ToList();

                //logger pour chaque plant
                foreach (var plant in orderedPlants)
                {
                    _Logger.LogInformation($"Ordered Plant: {plant.Name}, Type: {plant.Type}, Efficiency: {plant.Efficiency}, CalculatedFuelCost: {plant.CalculatedFuelCost}, CalculatedPMax: {plant.CalculatedPMax}");
                }
                return orderedPlants;
            }
            catch (Exception ex)
            {
                _Logger.LogError($"An error occurred while ordering power plants: {ex}");
                throw ex;
            }
        }

        public List<PlannedOutput> GetPlannedOutput(List<PowerPlant> powerPlants, Fuel fuel, decimal plannedLoad)
        {
            
            try
            {
                //appel aux PWPlants ordonné
                var orderedPowerPlants = OrderPowerPlants(powerPlants, fuel);

                var plannedOutputs = new List<PlannedOutput>();
                var remainingLoad = plannedLoad;

                foreach (var powerPlant in orderedPowerPlants)
                {
                    decimal plannedPower = 0;

                    //Si le Pmax calculé == 0 ou la PMin est supérieur à la charge demandé => PlannedPower = 0
                    if (powerPlant.CalculatedPMax == 0 || powerPlant.PMin > remainingLoad)
                    {
                        plannedPower = 0;
                    }
                    //Si la charge demandé est plus petite ou égale au PMax calculé et la charge est plus grande ou égale que le PMin alors on on assigne la charge demandé au plannedPower
                    else if (remainingLoad <= powerPlant.CalculatedPMax && remainingLoad >= powerPlant.PMin)
                    {
                        plannedPower = remainingLoad;
                        remainingLoad = 0;
                    }
                    //Si la charge excède le PMax, alors on assigne La PMax au plannedPower pour cette Plant, et la charge demandé restante est réduite par le PMax
                    else
                    {
                        plannedPower = powerPlant.CalculatedPMax;
                        remainingLoad -= powerPlant.CalculatedPMax;
                    }

                    plannedOutputs.Add(new PlannedOutput()
                    {
                        Name = powerPlant.Name,
                        P = plannedPower
                    });
                }

                //logger de chaque plant et son Power Output.
                foreach (var output in plannedOutputs)
                {
                    _Logger.LogInformation($"Planned Output: {output.Name}, Power: {output.P}");
                }

                return plannedOutputs;
            }
            catch (Exception ex)
            {
                // Log the exception
                _Logger.LogError($"An error occurred while calculating planned output: {ex}");
                throw ex;
            }
        }
    }
}
