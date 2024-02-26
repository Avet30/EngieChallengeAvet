using EngieChallenge.CORE.Models;
using EngieChallenge.CORE.Models.Enums;
using EngieChallenge.CORE.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallende.TESTS
{
    public class PowerPlantServiceTests
    {
        [Fact]
        public void CalculateRealCostAndPower_WithWindTurbine_ShouldCalculateCorrectly()
        {
            // Arrange
            var powerPlants = new List<PowerPlant>
                {
                    new PowerPlant {    Type = PowerPlantType.windturbine, 
                                        PMax = 100, 
                                        Efficiency = 0.5M 
                                    }
                };

            var fuel = new Fuel { Wind = 50 };
            var loggerMock = new Mock<ILogger<PowerPlantService>>();

            var sut = new PowerPlantService(loggerMock.Object);

            // Act
            var result = sut.CalculateRealCostAndPower(powerPlants, fuel);

            // Assert
            Assert.Single(result);
            Assert.Equal(50, result[0].CalculatedPMax);
            Assert.Equal(0.0M, result[0].CalculatedFuelCost);
        }

        [Fact]
        public void CalculateRealCostAndPower_WithGasfired_ShouldCalculateCorrectly()
        {
            // Arrange
            var powerPlants = new List<PowerPlant>
        {
            new PowerPlant { Type = PowerPlantType.gasfired, PMax = 100, Efficiency = 0.5M }
        };
            var fuel = new Fuel { Gas = 100 };

            var loggerMock = new Mock<ILogger<PowerPlantService>>();

            var sut = new PowerPlantService(loggerMock.Object);

            // Act
            var result = sut.CalculateRealCostAndPower(powerPlants, fuel);

            // Assert
            Assert.Single(result);
            Assert.Equal(100, result[0].CalculatedPMax);
            Assert.Equal(200, result[0].CalculatedFuelCost); // 100 (Gas) / 0.5 (Efficiency)
        }

        [Fact]
        public void CalculateRealCostAndPower_WithTurbojet_ShouldCalculateCorrectly()
        {
            // Arrange
            var powerPlants = new List<PowerPlant>
        {
            new PowerPlant { Type = PowerPlantType.turbojet, PMax = 100, Efficiency = 0.5M }
        };
            var fuel = new Fuel { Kerosine = 100 };

            var loggerMock = new Mock<ILogger<PowerPlantService>>();

            var sut = new PowerPlantService(loggerMock.Object);

            // Act
            var result = sut.CalculateRealCostAndPower(powerPlants, fuel);

            // Assert
            Assert.Single(result);
            Assert.Equal(100, result[0].CalculatedPMax);
            Assert.Equal(200, result[0].CalculatedFuelCost); // 100 (Kerosine) / 0.5 (Efficiency)
        }
    }
}
