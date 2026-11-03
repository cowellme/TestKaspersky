using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController([FromServices] IScanService service, ILogger<ScanController> logger) : ControllerBase
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
                logger.LogWarning(ex.Message);
                return BadRequest(ex);
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ScanTask>> GetById(Guid id, CancellationToken ct)
        {
            var task = await service.GetById(id, ct);
            if (task is null)
            {
                logger.LogWarning("Задача {ScanId} не найдена", id);
                return NotFound();
            }
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> ScanUrl(string url, CancellationToken cancellationToken)
        {
            try
            {
                var task = await service.CreateScan(url, cancellationToken);

                if (task == null)
                {
                    logger.LogWarning("Задача не создана");
                    return NotFound();
                }
                
                logger.LogInformation("Скан запущен");
                return Ok(task.Id);

            }
            catch (Exception ex)
            {
                logger.LogWarning("{ExMessage}\n\nUrl: {Url}", ex.Message, url);
                return BadRequest(ex);
            }
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<CancelScanResponse>> Cancel(Guid id, CancellationToken ct)
        {
            var result = await service.CancelScan(id, ct);
            if (result is null)
            {
                logger.LogWarning("Отмена: задача {ScanId} не найдена", id);
                return NotFound();
            }

            logger.LogInformation("Отмена задачи {ScanId}: {Success}", id, result.Success);
            return Ok(result);
        }
    }
}
