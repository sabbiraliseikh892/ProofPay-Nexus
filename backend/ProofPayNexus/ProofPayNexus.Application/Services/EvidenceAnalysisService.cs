
using ProofPayNexus.Application.DTOs.AI;
using ProofPayNexus.Application.Interfaces.AI;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Application.Interfaces.Storage;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System.Text.Json;

namespace ProofPayNexus.Application.Services
{
    public class EvidenceAnalysisService : IEvidenceAnalysisService
    {
        private readonly IEvidenceDocumentRepository _evidenceRepository;
        private readonly IEvidenceAnalysisRepository _analysisRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IGeminiService _geminiService;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EvidenceAnalysisService(
            IEvidenceDocumentRepository evidenceRepository,
            IEvidenceAnalysisRepository analysisRepository,
            IFileStorageService fileStorage,
            IGeminiService geminiService)
        {
            _evidenceRepository = evidenceRepository;
            _analysisRepository = analysisRepository;
            _fileStorage = fileStorage;
            _geminiService = geminiService;
        }

        public async Task<InvoiceExtractionResult> AnalyzeEvidenceAsync(
            Guid evidenceDocumentId,
            bool forceReanalysis = false,
            CancellationToken cancellationToken = default)
        {
            if (evidenceDocumentId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Evidence document ID is required.");
            }

            var evidenceDocument =
                await _evidenceRepository.GetByIdAsync(
                    evidenceDocumentId,
                    cancellationToken);

            if (evidenceDocument == null)
            {
                throw new KeyNotFoundException(
                    "Evidence document not found.");
            }

            var existingAnalysis =
                await _analysisRepository.GetLatestByEvidenceDocumentIdAsync(
                    evidenceDocumentId,
                    cancellationToken);

            if (!forceReanalysis &&
                existingAnalysis?.Status == EvidenceAnalysisStatus.Completed &&
                !string.IsNullOrWhiteSpace(existingAnalysis.ExtractedDataJson))
            {
                var existingResult =
                    JsonSerializer.Deserialize<InvoiceExtractionResult>(
                        existingAnalysis.ExtractedDataJson,
                        JsonOptions);

                if (existingResult != null)
                {
                    return existingResult;
                }
            }

            var analysis = new EvidenceAnalysis
            {
                Id = Guid.NewGuid(),
                EvidenceDocumentId = evidenceDocumentId,
                ModelName = "Gemini",
                Status = EvidenceAnalysisStatus.Processing,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _analysisRepository.AddAsync(
                analysis,
                cancellationToken);

            await _analysisRepository.SaveChangesAsync(
                cancellationToken);

            try
            {
                await using var documentStream =
                    await _fileStorage.OpenReadAsync(
                        evidenceDocument.StoredFileName,
                        cancellationToken);

                var geminiResponse =
                    await _geminiService.AnalyzeDocumentAsync(
                        documentStream,
                        evidenceDocument.ContentType,
                        cancellationToken);

                if (string.IsNullOrWhiteSpace(geminiResponse))
                {
                    throw new InvalidOperationException(
                        "Gemini returned an empty response.");
                }

                var extractionResult =
                    JsonSerializer.Deserialize<InvoiceExtractionResult>(
                        geminiResponse,
                        JsonOptions);

                if (extractionResult == null)
                {
                    throw new InvalidOperationException(
                        "Gemini returned an invalid extraction response.");
                }

                extractionResult.Items ??=
                    new List<InvoiceLineItemResult>();

                ValidateExtractionResult(extractionResult);

                analysis.ExtractedDataJson =
                    JsonSerializer.Serialize(
                        extractionResult,
                        JsonOptions);

                analysis.AnalysisSummary =
                    extractionResult.ExtractionNotes;

                analysis.ConfidenceScore =
                    extractionResult.ConfidenceScore;

                analysis.Status = EvidenceAnalysisStatus.Completed;

                analysis.CompletedAtUtc = DateTime.UtcNow;

                analysis.ErrorMessage = null;

                await _analysisRepository.SaveChangesAsync(
                    cancellationToken);

                return extractionResult;
            }
            catch (Exception exception)
            {
                analysis.Status = EvidenceAnalysisStatus.Failed;

                analysis.ErrorMessage = exception.Message;

                analysis.CompletedAtUtc = DateTime.UtcNow;

                try
                {
                    await _analysisRepository.SaveChangesAsync(
                        CancellationToken.None);
                }
                catch
                {
                    // Preserve the original exception.
                }

                throw;
            }
        }

        private static void ValidateExtractionResult(
            InvoiceExtractionResult result)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(result.InvoiceNumber))
            {
                errors.Add("Invoice number is missing.");
            }

            if (string.IsNullOrWhiteSpace(result.SupplierName))
            {
                errors.Add("Supplier name is missing.");
            }

            if (!result.InvoiceDate.HasValue)
            {
                errors.Add("Invoice date is missing.");
            }

            if (!result.TotalAmount.HasValue)
            {
                errors.Add("Invoice total amount is missing.");
            }

            if (result.TotalAmount.HasValue &&
                result.TotalAmount.Value < 0)
            {
                errors.Add("Invoice total amount cannot be negative.");
            }

            if (result.Items.Count == 0)
            {
                errors.Add("No genuine invoice line items were identified.");
            }

            foreach (var item in result.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                {
                    errors.Add("An invoice line item has no item name.");
                    continue;
                }

                if (item.Quantity.HasValue && item.Quantity.Value < 0)
                {
                    errors.Add(
                        $"Invalid quantity for item: {item.ItemName}.");
                }

                if (item.UnitPrice.HasValue && item.UnitPrice.Value < 0)
                {
                    errors.Add(
                        $"Invalid unit price for item: {item.ItemName}.");
                }

                if (item.LineTotal.HasValue && item.LineTotal.Value < 0)
                {
                    errors.Add(
                        $"Invalid line total for item: {item.ItemName}.");
                }
            }

            if (result.ConfidenceScore.HasValue &&
                (result.ConfidenceScore.Value < 0 ||
                 result.ConfidenceScore.Value > 100))
            {
                errors.Add("Invalid extraction confidence score.");
            }

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Invoice extraction validation failed: " +
                    string.Join(" ", errors));
            }
        }
    }
}
