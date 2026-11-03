using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Data;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VulnerabilitiesController([FromServices] IVulnerabilityService service) : ControllerBase
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

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var vulnerability = await service.GetById(id, cancellationToken);
                return vulnerability is null ? NotFound() : Ok(vulnerability);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateVulnerabilityRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var vulnerability = new Vulnerability
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    Severity = request.Severity,
                    Content = request.Content,
                    Url = request.Url,
                    CreatedAt = DateTime.UtcNow
                };

                await service.Add(vulnerability, cancellationToken);

                var result = await service.GetById(vulnerability.Id, cancellationToken);
                return result is null ? NotFound() : Ok(result.Id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await service.Delete(id, cancellationToken);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
