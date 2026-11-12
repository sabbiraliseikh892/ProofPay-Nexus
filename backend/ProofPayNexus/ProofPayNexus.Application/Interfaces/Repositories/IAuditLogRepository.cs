using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task AddAsync(
    AuditLog auditLog,
    CancellationToken cancellationToken = default);

        Task<List<AuditLog>> GetByEntityAsync(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
