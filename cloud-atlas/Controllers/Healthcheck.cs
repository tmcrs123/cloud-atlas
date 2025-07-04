using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HealthcheckController : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Healthcheck()
    {
        return Ok("cloud-atlas-api");
    }
}