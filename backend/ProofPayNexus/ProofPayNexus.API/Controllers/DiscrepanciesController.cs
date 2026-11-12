using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.DTOs.Discrepancies;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscrepanciesController : ControllerBase
    {
        private readonly IDiscrepancyService _discrepancyService;
        private readonly ILogger<DiscrepanciesController> _logger;

        public DiscrepanciesController(
            IDiscrepancyService discrepancyService,
            ILogger<DiscrepanciesController> logger)
        {
            _discrepancyService = discrepancyService;
            _logger = logger;
        }
        // GET: api/Discrepancies/purchase-order/{purchaseOrderId}
        [HttpGet("purchase-order/{purchaseOrderId:guid}")]
        public async Task<IActionResult> GetByPurchaseOrder(
            Guid purchaseOrderId,
            CancellationToken cancellationToken)
        {
            try
            {
                var discrepancies =
                    await _discrepancyService.GetByPurchaseOrderIdAsync(
                        purchaseOrderId,
                        cancellationToken);

                return Ok(discrepancies);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving discrepancies for purchase order {PurchaseOrderId}",
                    purchaseOrderId);

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving discrepancies."
                });
            }
        }

        // GET: api/Discrepancies/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var discrepancy =
                    await _discrepancyService.GetByIdAsync(
                        id,
                        cancellationToken);

                if (discrepancy == null)
                {
                    return NotFound(new
                    {
                        message = "Discrepancy not found."
                    });
                }

                return Ok(discrepancy);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving discrepancy {DiscrepancyId}",
                    id);

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving the discrepancy."
                });
            }
        }

        // PUT: api/Discrepancies/{id}/status
        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateDiscrepancyStatusRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Temporary reviewer identity.
                // Replace with authenticated user identity when authentication is implemented.
                var reviewedBy = User.Identity?.Name ?? "Admin";

                var updatedDiscrepancy =
                    await _discrepancyService.UpdateStatusAsync(
                        id,
                        request,
                        reviewedBy,
                        cancellationToken);

                return Ok(updatedDiscrepancy);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating discrepancy status for {DiscrepancyId}",
                    id);

                return StatusCode(500, new
                {
                    message = "An error occurred while updating the discrepancy."
                });
            }
        }
    }
}
