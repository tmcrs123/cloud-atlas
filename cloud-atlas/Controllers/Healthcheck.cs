using Microsoft.AspNetCore.Mvc;

public class HealthcheckController : BaseController
{
    public HealthcheckController()
    {
    }

    [HttpGet]
    public async Task<IActionResult> Healthcheck()
    {
        return Ok("cloud-atlas-api");
    }
}