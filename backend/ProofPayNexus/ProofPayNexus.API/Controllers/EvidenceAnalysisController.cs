using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.AI;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvidenceAnalysisController : ControllerBase
    {
        private readonly IEvidenceAnalysisService _analysisService;
        private readonly ILogger<EvidenceAnalysisController> _logger;

        public EvidenceAnalysisController(
            IEvidenceAnalysisService analysisService,
            ILogger<EvidenceAnalysisController> logger)
        {
            _analysisService = analysisService;
            _logger = logger;
        }
        // POST: api/evidence-analysis/{evidenceDocumentId}/analyze
        [HttpPost("{evidenceDocumentId:guid}/analyze")]
        [ProducesResponseType(
            typeof(InvoiceExtractionResult),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AnalyzeEvidence(
            Guid evidenceDocumentId,
            [FromQuery] bool forceReanalysis = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _analysisService.AnalyzeEvidenceAsync(
                    evidenceDocumentId,
                    forceReanalysis,
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new
                {
                    message = exception.Message
                });
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return StatusCode(499, new
                {
                    message = "The analysis request was cancelled."
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Gemini analysis failed for evidence document {EvidenceDocumentId}",
                    evidenceDocumentId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Evidence analysis failed. Please try again."
                    });
            }
        }
    }
}
