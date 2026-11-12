using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public AuditAction Action { get; set; }

        // Entity affected by the action
        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        // User or system responsible for the action
        public string PerformedBy { get; set; } = "System";

        public string? Description { get; set; }

        // Additional information in JSON format
        public string? MetadataJson { get; set; }

        public string? IpAddress { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}
