using Microsoft.AspNetCore.Mvc;

using XperienceCommunity.AccessibilityChecker.Models;
using XperienceCommunity.AccessibilityChecker.Scanning;

namespace XperienceCommunity.AccessibilityChecker.Controllers
{
    [ApiController]
    [Route("api/accessibility")]
    public sealed class AccessibilityScanController : ControllerBase
    {
        private readonly IAccessibilityScanService scanService;
        private readonly IScanResultRepository scanResultRepository;

        public AccessibilityScanController(IAccessibilityScanService scanService, IScanResultRepository scanResultRepository)
        {
            this.scanService = scanService;
            this.scanResultRepository = scanResultRepository;
        }

        [HttpGet("scans")]
        public async Task<IActionResult> GetScans(CancellationToken cancellationToken)
        {
            var results = await scanResultRepository.GetAllAsync(cancellationToken);
            return Ok(results);
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ScanRequestDto request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request?.Url))
            {
                return BadRequest(new ScanErrorDto { Code = "InvalidUrl", Message = "URL is required." });
            }

            var outcome = await scanService.ScanAsync(request.Url, cancellationToken);

            if (outcome.IsSuccess)
            {
                await scanResultRepository.UpsertAsync(outcome.Result!, cancellationToken);
            }

            return outcome switch
            {
                { IsSuccess: true } => Ok(outcome.Result),
                { ErrorCode: ScanErrorCode.InvalidUrl } =>
                    BadRequest(new ScanErrorDto { Code = "InvalidUrl", Message = outcome.ErrorMessage! }),
                { ErrorCode: ScanErrorCode.UnreachablePage } =>
                    StatusCode(StatusCodes.Status502BadGateway, new ScanErrorDto { Code = "UnreachablePage", Message = outcome.ErrorMessage! }),
                { ErrorCode: ScanErrorCode.Timeout } =>
                    StatusCode(StatusCodes.Status504GatewayTimeout, new ScanErrorDto { Code = "Timeout", Message = outcome.ErrorMessage! }),
                _ =>
                    StatusCode(StatusCodes.Status500InternalServerError, new ScanErrorDto { Code = "ScanFailed", Message = outcome.ErrorMessage ?? "The scan failed." })
            };
        }

        [HttpDelete("scan")]
        public async Task<IActionResult> DeleteScan([FromQuery] string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new ScanErrorDto { Code = "InvalidUrl", Message = "URL is required." });
            }

            await scanResultRepository.DeleteAsync(url, cancellationToken);
            return NoContent();
        }
    }
}
