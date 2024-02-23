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
            var result = _powerPlantService.GetPlannedOutput(request.Powerplants, request.Fuels, request.Load);

            if(result == null)
            {
                _Logger.LogError("Error while getting plannedOutput");
                return NotFound("Error while getting plannedOutput");
            }

            _Logger.LogInformation($"Planned output: {result}");
            return Ok(result);                     
        }
    }
}
