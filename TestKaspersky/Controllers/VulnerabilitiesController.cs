using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Data;
using TestKaspersky.Models;
using TestKaspersky.Services;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VulnerabilitiesController([FromServices] IVulnerabilityService service, ILogger<VulnerabilitiesController> logger) : ControllerBase
    {        
        /// <summary>
        /// Получение всех уязвимостей.
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
        /// Получение узявимости по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор узявимости.</param>
        /// <response code="200">Узявимость, статус и метаданные.</response>
        /// <response code="404">Узявимости не существует.</response>
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
                logger.LogWarning("{ExMessage}\n\nGuid: {Guid}", ex.Message, id);
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Создание узявимости.
        /// </summary>
        /// <param name="request">Обязательные поля title и severity</param>
        /// <response code="200">Узявимость, статус и метаданные.</response>
        /// <response code="404">Узявимости не существует.</response>
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
                logger.LogWarning("{ExMessage}\n\nGuid: {Title}", ex.Message, request.Title);
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Удаление узявимости.
        /// </summary>
        /// <param name="id">Guid уязвимости которую надо удалить</param>
        /// <response code="200">Узявимость удаленна.</response>
        /// <response code="404">Узявимости не существует.</response>
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
                logger.LogWarning("{ExMessage}\n\nGuid: {Guid}", ex.Message, id);
                return BadRequest(ex);
            }
        }
    }
}
