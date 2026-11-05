
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskFlow.Api.Diagnostics;
using TaskFlow.Api.Options;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/diagnostics")]
public sealed class DiagnosticsController(
    TransientOperation transient,
    ScopedOperation scoped,
    SingletonOperation singleton,
    OperationReporter reporter,
    ILogger<DiagnosticsController> logger,
    IOptions<DiagnosticsOptions> diagnosticsOptions) : ControllerBase
{
    private bool IsEnabled => diagnosticsOptions.Value.Enabled;

    [HttpGet("lifetimes")]
    [ProducesResponseType<LifetimesReport>(StatusCodes.Status200OK)]
    public ActionResult<LifetimesReport> GetLifetimes()
    {
        if (!IsEnabled)
            return NotFound();

        var fromController = new OperationIds(
            transient.Id,
            scoped.Id,
            singleton.Id);

        return new LifetimesReport(fromController, reporter.Report());
    }

    [HttpGet("slow")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Slow(
        int seconds = 5,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
            return NotFound();

        logger.LogInformation("Slow request started for {Seconds} s", seconds);

        await Task.Delay(
            TimeSpan.FromSeconds(seconds),
            cancellationToken);

        logger.LogInformation("Slow request finished after {Seconds} s", seconds);

        return Ok(new { seconds });
    }

    [HttpGet("boom")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public IActionResult Boom(string? kind)
    {
        if (!IsEnabled)
            return NotFound();

        if (kind == "argument")
            throw new ArgumentException("The supplied argument is invalid.");

        throw new InvalidOperationException(
            "Test exception from DiagnosticsController.");
    }
}
