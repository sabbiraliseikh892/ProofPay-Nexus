using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.Evidence;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Enums;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvidenceController : ControllerBase
    {
        private readonly IEvidenceDocumentService _evidenceService;

        private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

        public EvidenceController(
            IEvidenceDocumentService evidenceService)
        {
            _evidenceService = evidenceService;
        }

        // POST: api/evidence/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxFileSizeInBytes)]
        [RequestFormLimits(
            MultipartBodyLengthLimit = MaxFileSizeInBytes)]
        [ProducesResponseType(
            typeof(EvidenceDocumentResponse),
            StatusCodes.Status201Created)]
        public async Task<IActionResult> Upload(
            [FromForm] Guid purchaseOrderId,
            [FromForm] EvidenceDocumentType documentType,
            IFormFile file,
            [FromForm] string? uploadedBy,
            CancellationToken cancellationToken)
        {
            if (file == null || file.Length <= 0)
            {
                return BadRequest(new
                {
                    message = "Please select a valid file."
                });
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                return BadRequest(new
                {
                    message = "Maximum file size allowed is 10 MB."
                });
            }

            await using var fileStream = file.OpenReadStream();

            var request = new UploadEvidenceRequest
            {
                PurchaseOrderId = purchaseOrderId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeInBytes = file.Length,
                DocumentType = documentType,
                UploadedBy = uploadedBy,
                FileStream = fileStream
            };

            var response = await _evidenceService.UploadAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        // GET: api/evidence/purchase-order/{purchaseOrderId}
        [HttpGet("purchase-order/{purchaseOrderId:guid}")]
        [ProducesResponseType(
            typeof(List<EvidenceDocumentResponse>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPurchaseOrderId(
            Guid purchaseOrderId,
            CancellationToken cancellationToken)
        {
            var response =
                await _evidenceService.GetByPurchaseOrderIdAsync(
                    purchaseOrderId,
                    cancellationToken);

            return Ok(response);
        }

        // GET: api/evidence/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(EvidenceDocumentResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var response = await _evidenceService.GetByIdAsync(
                id,
                cancellationToken);

            if (response == null)
            {
                return NotFound(new
                {
                    message = "Evidence document not found."
                });
            }

            return Ok(response);
        }
    }
}
