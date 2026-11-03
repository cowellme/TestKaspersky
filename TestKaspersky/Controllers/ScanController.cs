using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Services;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController([FromServices] IScanService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await service.GetAll(cancellationToken);
                return result is { Count: > 0 } ? Ok(result) : NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ScanUrl(string url, CancellationToken cancellationToken)
        {
            try
            {
                await service.CreateScan(url, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
