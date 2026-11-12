using Microsoft.EntityFrameworkCore;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Persistence.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            AuditLog auditLog,
            CancellationToken cancellationToken = default)
        {
            await _context.AuditLogs.AddAsync(
                auditLog,
                cancellationToken);
        }

        public async Task<List<AuditLog>> GetByEntityAsync(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Where(x =>
                    x.EntityType == entityType &&
                    x.EntityId == entityId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
