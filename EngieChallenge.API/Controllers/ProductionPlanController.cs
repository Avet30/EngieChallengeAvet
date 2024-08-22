using EngieChallenge.API.Models;
using EngieChallenge.CORE.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EngieChallenge.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductionPlanController : ControllerBase
{
    private readonly IPowerPlantService _powerPlantService;
    

    public ProductionPlanController(IPowerPlantService powerPlantService)
    {
        _powerPlantService = powerPlantService;
    }

    [HttpPost]
    public IActionResult Compute([FromBody] Payload request)
    {
        var plannedOutput = _powerPlantService.GetProductionPlan(request.Powerplants, request.Fuels, request.Load);
        return Ok(plannedOutput);
    }
}
