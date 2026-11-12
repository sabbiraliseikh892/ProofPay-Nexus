using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.DTOs.Evidence
{
    public class EvidenceDocumentResponse
    {
        public Guid Id { get; set; }

        public Guid PurchaseOrderId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSizeInBytes { get; set; }

        public EvidenceDocumentType DocumentType { get; set; }

        public string? UploadedBy { get; set; }

        public DateTime UploadedAtUtc { get; set; }
    }
}
