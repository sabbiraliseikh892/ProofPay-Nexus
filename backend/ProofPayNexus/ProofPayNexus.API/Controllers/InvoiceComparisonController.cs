using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.AI;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceComparisonController : ControllerBase
    {
        private readonly IInvoiceComparisonService _comparisonService;
        private readonly ILogger<InvoiceComparisonController> _logger;

        public InvoiceComparisonController(
            IInvoiceComparisonService comparisonService,
            ILogger<InvoiceComparisonController> logger)
        {
            _comparisonService = comparisonService;
            _logger = logger;
        }
        // POST: api/invoice-comparison/{evidenceDocumentId}/compare
        [HttpPost("{evidenceDocumentId:guid}/compare")]
        [ProducesResponseType(
            typeof(PurchaseOrderComparisonResult),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Compare(
            Guid evidenceDocumentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _comparisonService.CompareAsync(
                    evidenceDocumentId,
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
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    message = exception.Message
                });
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return StatusCode(499, new
                {
                    message = "The comparison request was cancelled."
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Invoice comparison failed for evidence document {EvidenceDocumentId}",
                    evidenceDocumentId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Invoice comparison failed. Please try again."
                    });
            }
        }
    }
}
