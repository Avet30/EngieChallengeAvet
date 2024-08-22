using EngieChallenge.API.Models;
using EngieChallenge.CORE.Domain.Exceptions;
using EngieChallenge.CORE.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Compute([FromBody] Payload request)
        {

            var plannedOutput = _powerPlantService.GetProductionPlan(request.Powerplants, request.Fuels, request.Load);

            return Ok(plannedOutput);
        }
    }
}
