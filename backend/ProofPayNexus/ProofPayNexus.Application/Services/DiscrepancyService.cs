
using System.Text.Json;
using ProofPayNexus.Application.DTOs.Discrepancies;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;

namespace ProofPayNexus.Application.Services
{
    public class DiscrepancyService : IDiscrepancyService
    {
        private readonly IDiscrepancyRepository _discrepancyRepository;
        private readonly IAuditLogService _auditLogService;

        public DiscrepancyService(
            IDiscrepancyRepository discrepancyRepository,
            IAuditLogService auditLogService)
        {
            _discrepancyRepository = discrepancyRepository;
            _auditLogService = auditLogService;
        }

        public async Task<List<Discrepancy>> GetByPurchaseOrderIdAsync(
            Guid purchaseOrderId,
            CancellationToken cancellationToken = default)
        {
            if (purchaseOrderId == Guid.Empty)
                throw new ArgumentException("Invalid purchase order ID.");

            return await _discrepancyRepository
                .GetByPurchaseOrderIdAsync(
                    purchaseOrderId,
                    cancellationToken);
        }

        public async Task<Discrepancy?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid discrepancy ID.");

            return await _discrepancyRepository
                .GetByIdAsync(id, cancellationToken);
        }

        public async Task<Discrepancy> UpdateStatusAsync(
            Guid id,
            UpdateDiscrepancyStatusRequest request,
            string reviewedBy,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid discrepancy ID.");

            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(reviewedBy))
                throw new ArgumentException("Reviewer information is required.");

            if (!Enum.IsDefined(typeof(DiscrepancyStatus), request.Status))
                throw new ArgumentException("Invalid discrepancy status.");

            var discrepancy = await _discrepancyRepository
                .GetByIdAsync(id, cancellationToken);

            if (discrepancy == null)
                throw new KeyNotFoundException("Discrepancy not found.");

            var previousStatus = discrepancy.Status;

            // Validate status transitions.
            bool isValidTransition = false;

            if (previousStatus == DiscrepancyStatus.Open)
            {
                if (request.Status == DiscrepancyStatus.UnderReview)
                {
                    isValidTransition = true;
                }
            }
            else if (previousStatus == DiscrepancyStatus.UnderReview)
            {
                if (request.Status == DiscrepancyStatus.UnderReview ||
                    request.Status == DiscrepancyStatus.Resolved ||
                    request.Status == DiscrepancyStatus.Accepted ||
                    request.Status == DiscrepancyStatus.Rejected)
                {
                    isValidTransition = true;
                }
            }

            if (!isValidTransition)
            {
                throw new InvalidOperationException(
                    $"Cannot change discrepancy status from " +
                    $"{previousStatus} to {request.Status}.");
            }

            // Avoid duplicate audit entries for unchanged status.
            if (previousStatus == request.Status)
                return discrepancy;

            bool isFinalStatus =
                request.Status == DiscrepancyStatus.Resolved ||
                request.Status == DiscrepancyStatus.Accepted ||
                request.Status == DiscrepancyStatus.Rejected;

            if (isFinalStatus &&
                string.IsNullOrWhiteSpace(request.ResolutionNotes))
            {
                throw new ArgumentException(
                    "Resolution notes are required for a final decision.");
            }

            discrepancy.Status = request.Status;
            discrepancy.ResolutionNotes = request.ResolutionNotes?.Trim();

            if (isFinalStatus)
            {
                discrepancy.ResolvedAtUtc = DateTime.UtcNow;
            }

            // Determine audit action.
            AuditAction auditAction;

            if (request.Status == DiscrepancyStatus.UnderReview)
            {
                auditAction = AuditAction.DiscrepancyReviewStarted;
            }
            else if (request.Status == DiscrepancyStatus.Resolved)
            {
                auditAction = AuditAction.DiscrepancyResolved;
            }
            else if (request.Status == DiscrepancyStatus.Accepted)
            {
                auditAction = AuditAction.DiscrepancyAccepted;
            }
            else if (request.Status == DiscrepancyStatus.Rejected)
            {
                auditAction = AuditAction.DiscrepancyRejected;
            }
            else
            {
                throw new InvalidOperationException(
                    "Unsupported discrepancy status.");
            }

            // Prepare audit metadata.
            var metadata = JsonSerializer.Serialize(new
            {
                PreviousStatus = previousStatus.ToString(),
                NewStatus = request.Status.ToString(),
                ResolutionNotes = discrepancy.ResolutionNotes
            });

            // Add audit record.
            await _auditLogService.CreateAsync(
                auditAction,
                "Discrepancy",
                discrepancy.Id,
                reviewedBy,
                $"Discrepancy status changed from {previousStatus} to {request.Status}.",
                metadata,
                cancellationToken);

            // Save discrepancy and audit record together.
            await _discrepancyRepository.SaveChangesAsync(
                cancellationToken);

            return discrepancy;
        }
    }
}
