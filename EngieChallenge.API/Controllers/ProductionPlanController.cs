using EngieChallenge.CORE.Exceptions;
using EngieChallenge.CORE.Interfaces;
using EngieChallenge.CORE.Models;
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
                var plannedOutput = _powerPlantService.GetPlannedOutput(request.Powerplants, request.Fuels, request.Load);

                // Return the calculated planned output
                return Ok(plannedOutput);
            }
            catch (PlannedOutputCalculationException ex)
            {
                // Log the exception (if needed)
                _Logger.LogError($"An error occurred while calculating planned output: {ex.Message}");

                // Return a custom error response without the stack trace
                return BadRequest("Unable to calculate planned output. Remaining load cannot be fulfilled.");
            }
        }
    }
}
