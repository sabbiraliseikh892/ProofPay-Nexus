using ProofPayNexus.Application.DTOs.AuditLogs;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task CreateAsync(
       AuditAction action,
       string entityType,
       Guid entityId,
       string performedBy,
       string? description = null,
       string? metadataJson = null,
       CancellationToken cancellationToken = default);

        Task<List<AuditLogResponse>> GetByEntityAsync(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken = default);
    }
}
