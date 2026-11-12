using ProofPayNexus.Application.DTOs.AuditLogs;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(
            IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }
        public async Task CreateAsync(
       AuditAction action,
       string entityType,
       Guid entityId,
       string performedBy,
       string? description = null,
       string? metadataJson = null,
       CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(AuditAction), action))
            {
                throw new ArgumentException("Invalid audit action.");
            }

            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type is required.");
            }

            if (entityId == Guid.Empty)
            {
                throw new ArgumentException("Entity ID is required.");
            }

            if (string.IsNullOrWhiteSpace(performedBy))
            {
                throw new ArgumentException("Performed by is required.");
            }

            // Validate metadata JSON when provided.
            if (!string.IsNullOrWhiteSpace(metadataJson))
            {
                using var document = JsonDocument.Parse(metadataJson);
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityType = entityType.Trim(),
                EntityId = entityId,
                PerformedBy = performedBy.Trim(),
                Description = description?.Trim(),
                MetadataJson = metadataJson,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(
                auditLog,
                cancellationToken);

            //await _auditLogRepository.SaveChangesAsync(
            //    cancellationToken);
        }

        public async Task<List<AuditLogResponse>> GetByEntityAsync(
            string entityType,
            Guid entityId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                throw new ArgumentException("Entity type is required.");
            }

            if (entityId == Guid.Empty)
            {
                throw new ArgumentException("Entity ID is required.");
            }

            var auditLogs = await _auditLogRepository.GetByEntityAsync(
                entityType.Trim(),
                entityId,
                cancellationToken);

            return auditLogs.Select(x => new AuditLogResponse
            {
                Id = x.Id,
                Action = x.Action.ToString(),
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                PerformedBy = x.PerformedBy,
                Description = x.Description,
                MetadataJson = x.MetadataJson,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList();
        }
    }
}
