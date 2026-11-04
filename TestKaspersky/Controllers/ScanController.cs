using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController([FromServices] IScanService service, ILogger<ScanController> logger) : ControllerBase
    {
        /// <summary>
        /// Получение всех сохраненных задач.
        /// </summary>
        /// <response code="200">Задачи, статусы и метаданные.</response>
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

        /// <summary>
        /// Возвращает текущий статус задачи.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <response code="200">Задача, статус и метаданные.</response>
        /// <response code="404">Задчи не существует.</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ScanTask>> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var task = await service.GetById(id, ct);
                if (task is null)
                {
                    logger.LogWarning("Задача {ScanId} не найдена", id);
                    return NotFound();
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Запускает фоновую задачу сканирования по ссылке (мок сервис) на уязвимости.
        /// </summary>
        /// <param name="url">Ссылка.</param>
        /// <param name="cancellationToken">Токен отмены запроса.</param>
        /// <response code="200">Идентификатор задачи.</response>
        /// <response code="404">Задача не создана.</response>
        /// <response code="400">Некорректный запрос.</response>
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

        /// <summary>
        /// Отменяет задачу в очереди или уже выполняющуюся.
        /// </summary>
        /// <param name="id">Идентификатор задачи.</param>
        /// <response code="200">Результат отмены: идентификатор, время и успешность.</response>
        /// <response code="404">Задача не найдена.</response>
        [HttpPost("{id:guid}/cancel")]
        public async Task<ActionResult<CancelScanResponse>> Cancel(Guid id, CancellationToken ct)
        {
            try
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
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return BadRequest(ex);
            }
        }
    }
}
