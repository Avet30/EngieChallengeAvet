using EngieChallenge.API.Models;
using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace EngieChallenge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IPowerPlantService _powerPlantService;
        private readonly ILogger<ProductionPlanController> _Logger;

        public ProductionPlanController(IPowerPlantService powerPlantService, ILogger<ProductionPlanController> logger)
        {
            _powerPlantService = powerPlantService;
            _Logger = logger;
        }

        [HttpPost]
        public IActionResult Calculate([FromBody] Payload request)
        {          
            try
            {
                // Call the service method to calculate planned output
                var plannedOutput = _powerPlantService.GetProductionPlan(request.Powerplants, request.Fuels, request.Load);

                return Ok(plannedOutput);
            }
            catch (PlannedOutputCalculationException ex)
            {
                _Logger.LogError($"An error occurred while calculating planned output: {ex.Message}");
                return BadRequest("Unable to calculate planned output. Remaining load cannot be fulfilled.");
            }
        }
    }
}
