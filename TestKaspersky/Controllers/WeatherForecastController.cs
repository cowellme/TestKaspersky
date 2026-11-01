using Microsoft.AspNetCore.Mvc;
using TestKaspersky.Data;

namespace TestKaspersky.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(WeatherForecastRepository repository) : ControllerBase
    {
        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IReadOnlyList<WeatherForecast>> Get(CancellationToken cancellationToken)
            => await repository.GetAllAsync(cancellationToken);

        [HttpPost]
        public async Task<ActionResult<WeatherForecast>> Add(WeatherForecast forecast, CancellationToken cancellationToken)
        {
            await repository.AddAsync(forecast, cancellationToken);
            return CreatedAtAction(nameof(Get), null, forecast);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}
