using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.AuditLogs
{
    public class AuditLogResponse
    {
        public Guid Id { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public string PerformedBy { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? MetadataJson { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
