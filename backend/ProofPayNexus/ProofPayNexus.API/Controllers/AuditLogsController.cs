using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProofPayNexus.Application.Interfaces.Services;

namespace ProofPayNexus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(
            IAuditLogService auditLogService,
            ILogger<AuditLogsController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        // GET: api/AuditLogs/Discrepancy/{entityId}
        [HttpGet("{entityType}/{entityId:guid}")]
        public async Task<IActionResult> GetByEntity(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken)
        {
            try
            {
                var auditLogs = await _auditLogService.GetByEntityAsync(
                    entityType,
                    entityId,
                    cancellationToken);

                return Ok(auditLogs);
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
                    "Error retrieving audit logs for {EntityType} {EntityId}",
                    entityType,
                    entityId);

                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving audit history."
                });
            }
        }
    }
}
