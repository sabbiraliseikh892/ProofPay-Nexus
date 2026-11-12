
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentAssessmentsController : ControllerBase
{
    private readonly IPaymentAssessmentService _paymentAssessmentService;
    private readonly ILogger<PaymentAssessmentsController> _logger;

    public PaymentAssessmentsController(
        IPaymentAssessmentService paymentAssessmentService,
        ILogger<PaymentAssessmentsController> logger)
    {
        _paymentAssessmentService = paymentAssessmentService;
        _logger = logger;
    }

    // POST: api/PaymentAssessments/{purchaseOrderId}/assess
    [HttpPost("{purchaseOrderId:guid}/assess")]
    public async Task<IActionResult> Assess(
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentAssessmentService.AssessAsync(
                purchaseOrderId,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error assessing payment risk for purchase order {PurchaseOrderId}",
                purchaseOrderId);

            return StatusCode(500, new
            {
                message = "An error occurred while assessing payment risk."
            });
        }
    }

    // GET: api/PaymentAssessments/purchase-order/{purchaseOrderId}
    [HttpGet("purchase-order/{purchaseOrderId:guid}")]
    public async Task<IActionResult> GetLatest(
        Guid purchaseOrderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentAssessmentService
                .GetLatestByPurchaseOrderIdAsync(
                    purchaseOrderId,
                    cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Payment assessment not found."
                });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error retrieving payment assessment for purchase order {PurchaseOrderId}",
                purchaseOrderId);

            return StatusCode(500, new
            {
                message = "An error occurred while retrieving payment assessment."
            });
        }
    }
}
