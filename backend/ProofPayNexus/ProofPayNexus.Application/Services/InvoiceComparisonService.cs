using ProofPayNexus.Application.DTOs.AI;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Services
{
    public class InvoiceComparisonService : IInvoiceComparisonService
    {
        private readonly IEvidenceDocumentRepository _evidenceRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IEvidenceAnalysisRepository _analysisRepository;
        private readonly IDiscrepancyRepository _discrepancyRepository;

        private const decimal MoneyTolerance = 0.01m;
        private const decimal QuantityTolerance = 0.0001m;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public InvoiceComparisonService(
        IEvidenceDocumentRepository evidenceRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IEvidenceAnalysisRepository analysisRepository,
        IDiscrepancyRepository discrepancyRepository)
        {
            _evidenceRepository = evidenceRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _analysisRepository = analysisRepository;
            _discrepancyRepository = discrepancyRepository;
        }
        public async Task<PurchaseOrderComparisonResult> CompareAsync(
        Guid evidenceDocumentId,
        CancellationToken cancellationToken = default)
        {
            if (evidenceDocumentId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Evidence document ID is required.");
            }

            // 1. Retrieve evidence document.
            var evidence = await _evidenceRepository.GetByIdAsync(
                evidenceDocumentId,
                cancellationToken);

            if (evidence == null)
            {
                throw new KeyNotFoundException(
                    "Evidence document not found.");
            }

            // 2. Retrieve associated purchase order and items.
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
                evidence.PurchaseOrderId,
                cancellationToken);

            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException(
                    "Associated purchase order not found.");
            }

            // 3. Retrieve latest Gemini analysis.
            var analysis =
                await _analysisRepository.GetLatestByEvidenceDocumentIdAsync(
                    evidenceDocumentId,
                    cancellationToken);

            if (analysis == null ||
                analysis.Status != EvidenceAnalysisStatus.Completed ||
                string.IsNullOrWhiteSpace(analysis.ExtractedDataJson))
            {
                throw new InvalidOperationException(
                    "A completed Gemini analysis is required before comparison.");
            }

            // 4. Deserialize extracted invoice data.
            var invoice = JsonSerializer.Deserialize<InvoiceExtractionResult>(
                analysis.ExtractedDataJson,
                JsonOptions);

            if (invoice == null)
            {
                throw new InvalidOperationException(
                    "Stored invoice extraction data is invalid.");
            }

            if (invoice.Items == null || invoice.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Invoice contains no extracted line items.");
            }

            var poItems = purchaseOrder.Items.ToList();

            var result = new PurchaseOrderComparisonResult
            {
                PurchaseOrderId = purchaseOrder.Id,
                EvidenceDocumentId = evidenceDocumentId,
                PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber,
                InvoiceNumber = invoice.InvoiceNumber,
                SupplierName = invoice.SupplierName,
                PurchaseOrderTotal = purchaseOrder.TotalAmount,
                InvoiceTotal = invoice.TotalAmount,
                TotalDifference = invoice.TotalAmount.HasValue
                    ? invoice.TotalAmount.Value - purchaseOrder.TotalAmount
                    : null
            };

            // 5. Compare supplier names.
            result.IsSupplierMatching = AreNamesEqual(
                purchaseOrder.SupplierName,
                invoice.SupplierName);

            if (!result.IsSupplierMatching)
            {
                result.Findings.Add(
                    "Supplier name does not match the purchase order.");
            }

            // 6. Compare overall invoice and purchase order totals.
            result.IsTotalMatching =
                invoice.TotalAmount.HasValue &&
                Math.Abs(
                    invoice.TotalAmount.Value - purchaseOrder.TotalAmount
                ) <= MoneyTolerance;

            if (!result.IsTotalMatching)
            {
                result.Findings.Add(
                    "Invoice total does not match the purchase order total.");
            }

            // 7. Track purchase order items already matched.
            var matchedPurchaseOrderItemIds = new HashSet<Guid>();

            foreach (var invoiceItem in invoice.Items)
            {
                var candidates = FindMatchingItems(
                    poItems,
                    invoiceItem,
                    matchedPurchaseOrderItemIds);

                if (candidates.Count == 0)
                {
                    result.Items.Add(new InvoiceItemComparisonResult
                    {
                        InvoiceItemName = invoiceItem.ItemName,
                        SKU = invoiceItem.SKU,
                        InvoiceQuantity = invoiceItem.Quantity,
                        InvoiceUnitPrice = invoiceItem.UnitPrice,
                        InvoiceLineTotal = invoiceItem.LineTotal,
                        IsMatching = false,
                        Discrepancies = new List<string>
                    {
                        "Invoice item does not match any purchase order item."
                    }
                    });

                    result.Findings.Add(
                        $"Unexpected invoice item: {invoiceItem.ItemName ?? "Unknown"}.");

                    continue;
                }

                if (candidates.Count > 1)
                {
                    result.Items.Add(new InvoiceItemComparisonResult
                    {
                        InvoiceItemName = invoiceItem.ItemName,
                        SKU = invoiceItem.SKU,
                        InvoiceQuantity = invoiceItem.Quantity,
                        InvoiceUnitPrice = invoiceItem.UnitPrice,
                        InvoiceLineTotal = invoiceItem.LineTotal,
                        IsMatching = false,
                        Discrepancies = new List<string>
                    {
                        "Multiple purchase order items match this invoice item. Manual review is required."
                    }
                    });

                    result.Findings.Add(
                        $"Ambiguous item match: {invoiceItem.ItemName ?? "Unknown"}.");

                    continue;
                }

                var poItem = candidates[0];

                matchedPurchaseOrderItemIds.Add(poItem.Id);

                var comparison = CompareItem(poItem, invoiceItem);

                result.Items.Add(comparison);

                if (!comparison.IsMatching)
                {
                    result.Findings.Add(
                        $"Discrepancy detected for item: {poItem.ItemName}.");
                }
            }

            // 8. Identify purchase order items missing from the invoice.
            foreach (var poItem in poItems)
            {
                if (matchedPurchaseOrderItemIds.Contains(poItem.Id))
                    continue;

                result.Items.Add(new InvoiceItemComparisonResult
                {
                    PurchaseOrderItemName = poItem.ItemName,
                    SKU = poItem.SKU,
                    PurchaseOrderQuantity = poItem.Quantity,
                    PurchaseOrderUnitPrice = poItem.UnitPrice,
                    PurchaseOrderLineTotal = poItem.LineTotal,
                    IsMatching = false,
                    Discrepancies = new List<string>
                {
                    "Purchase order item is missing from the invoice."
                }
                });

                result.Findings.Add(
                    $"Missing invoice item: {poItem.ItemName}.");
            }

            // 9. Calculate comparison summary.
            result.TotalItemsCompared = result.Items.Count;

            result.MatchingItems = result.Items.Count(
                x => x.IsMatching);

            result.DiscrepantItems =
                result.TotalItemsCompared - result.MatchingItems;

            result.RequiresReview =
                !result.IsSupplierMatching ||
                !result.IsTotalMatching ||
                result.DiscrepantItems > 0;

            // Persist detected discrepancies in SQL Server.
            await SaveDiscrepanciesAsync(
                result,
                cancellationToken);

            return result;
        }
        private static List<Domain.Entities.PurchaseOrderItem>
        FindMatchingItems(
            List<Domain.Entities.PurchaseOrderItem> poItems,
            InvoiceLineItemResult invoiceItem,
            HashSet<Guid> matchedIds)
        {
            // First preference: exact SKU match.
            if (!string.IsNullOrWhiteSpace(invoiceItem.SKU))
            {
                var skuMatches = poItems
                    .Where(x =>
                        !matchedIds.Contains(x.Id) &&
                        !string.IsNullOrWhiteSpace(x.SKU) &&
                        string.Equals(
                            Normalize(x.SKU),
                            Normalize(invoiceItem.SKU),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (skuMatches.Count > 0)
                    return skuMatches;
            }

            // Second preference: exact normalized item name.
            if (!string.IsNullOrWhiteSpace(invoiceItem.ItemName))
            {
                return poItems
                    .Where(x =>
                        !matchedIds.Contains(x.Id) &&
                        string.Equals(
                            Normalize(x.ItemName),
                            Normalize(invoiceItem.ItemName),
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return new List<Domain.Entities.PurchaseOrderItem>();
        }

        private static InvoiceItemComparisonResult CompareItem(
            Domain.Entities.PurchaseOrderItem poItem,
            InvoiceLineItemResult invoiceItem)
        {
            var discrepancies = new List<string>();

            var quantityMatches =
                invoiceItem.Quantity.HasValue &&
                Math.Abs(
                    poItem.Quantity - invoiceItem.Quantity.Value
                ) <= QuantityTolerance;

            var unitPriceMatches =
                invoiceItem.UnitPrice.HasValue &&
                Math.Abs(
                    poItem.UnitPrice - invoiceItem.UnitPrice.Value
                ) <= MoneyTolerance;

            var lineTotalMatches =
                invoiceItem.LineTotal.HasValue &&
                Math.Abs(
                    poItem.LineTotal - invoiceItem.LineTotal.Value
                ) <= MoneyTolerance;

            if (!quantityMatches)
                discrepancies.Add("Quantity mismatch.");

            if (!unitPriceMatches)
                discrepancies.Add("Unit price mismatch.");

            if (!lineTotalMatches)
                discrepancies.Add("Line total mismatch.");

            return new InvoiceItemComparisonResult
            {
                PurchaseOrderItemName = poItem.ItemName,
                InvoiceItemName = invoiceItem.ItemName,
                SKU = invoiceItem.SKU ?? poItem.SKU,

                PurchaseOrderQuantity = poItem.Quantity,
                InvoiceQuantity = invoiceItem.Quantity,

                PurchaseOrderUnitPrice = poItem.UnitPrice,
                InvoiceUnitPrice = invoiceItem.UnitPrice,

                PurchaseOrderLineTotal = poItem.LineTotal,
                InvoiceLineTotal = invoiceItem.LineTotal,

                IsMatching = discrepancies.Count == 0,
                Discrepancies = discrepancies
            };
        }

        private static bool AreNamesEqual(
            string? first,
            string? second)
        {
            if (string.IsNullOrWhiteSpace(first) ||
                string.IsNullOrWhiteSpace(second))
            {
                return false;
            }

            return string.Equals(
                Normalize(first),
                Normalize(second),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string value)
        {
            return string.Join(
                " ",
                value.Trim().Split(
                    (char[]?)null,
                    StringSplitOptions.RemoveEmptyEntries));
        }

        private async Task SaveDiscrepanciesAsync(
            PurchaseOrderComparisonResult result,
            CancellationToken cancellationToken)
        {
            var existingDiscrepancies =
                await _discrepancyRepository.GetByEvidenceDocumentIdAsync(
                    result.EvidenceDocumentId,
                    cancellationToken);

            var newDiscrepancies = new List<Discrepancy>();

            void AddIfNew(
                DiscrepancyType type,
                DiscrepancySeverity severity,
                string title,
                string description,
                string? expectedValue,
                string? actualValue,
                string recommendedAction)
            {
                // Do not create another record for an existing issue.
                // This also preserves resolved and accepted records.
                var alreadyExists = existingDiscrepancies.Any(x =>
                    x.Type == type &&
                    string.Equals(
                        x.Title,
                        title,
                        StringComparison.OrdinalIgnoreCase));

                if (alreadyExists)
                    return;

                newDiscrepancies.Add(new Discrepancy
                {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = result.PurchaseOrderId,
                    EvidenceDocumentId = result.EvidenceDocumentId,
                    Type = type,
                    Severity = severity,
                    Status = DiscrepancyStatus.Open,
                    Title = title,
                    Description = description,
                    ExpectedValue = expectedValue,
                    ActualValue = actualValue,
                    RecommendedAction = recommendedAction,
                    IsAiDetected = false,
                    DetectedAtUtc = DateTime.UtcNow
                });
            }

            // 1. Supplier mismatch.
            if (!result.IsSupplierMatching)
            {
                AddIfNew(
                    DiscrepancyType.InvoiceMismatch,
                    DiscrepancySeverity.High,
                    "Supplier mismatch",
                    "The supplier name extracted from the invoice does not match the purchase order supplier.",
                    "Purchase order supplier",
                    result.SupplierName,
                    "Verify the supplier identity before approving payment.");
            }

            // 2. Overall invoice total mismatch.
            if (!result.IsTotalMatching)
            {
                AddIfNew(
                    DiscrepancyType.InvoiceMismatch,
                    DiscrepancySeverity.Critical,
                    "Invoice total mismatch",
                    "The invoice total does not match the purchase order total.",
                    result.PurchaseOrderTotal.ToString(
                        "0.00", CultureInfo.InvariantCulture),
                    result.InvoiceTotal?.ToString(
                        "0.00", CultureInfo.InvariantCulture),
                    "Review the invoice amount and obtain clarification from the supplier.");
            }

            // 3. Item-level discrepancies.
            foreach (var item in result.Items)
            {
                foreach (var issue in item.Discrepancies)
                {
                    if (issue.Contains(
                        "Quantity mismatch",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfNew(
                            DiscrepancyType.QuantityMismatch,
                            DiscrepancySeverity.High,
                            $"Quantity mismatch: {item.PurchaseOrderItemName}",
                            issue,
                            item.PurchaseOrderQuantity?.ToString(
                                CultureInfo.InvariantCulture),
                            item.InvoiceQuantity?.ToString(
                                CultureInfo.InvariantCulture),
                            "Verify the delivered and invoiced quantities.");
                    }
                    else if (issue.Contains(
                        "Unit price mismatch",
                        StringComparison.OrdinalIgnoreCase) ||
                        issue.Contains(
                        "Line total mismatch",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfNew(
                            DiscrepancyType.PriceMismatch,
                            DiscrepancySeverity.High,
                            $"Price mismatch: {item.PurchaseOrderItemName}",
                            issue,
                            item.PurchaseOrderUnitPrice?.ToString(
                                "0.00", CultureInfo.InvariantCulture),
                            item.InvoiceUnitPrice?.ToString(
                                "0.00", CultureInfo.InvariantCulture),
                            "Verify the agreed unit price and invoice calculation.");
                    }
                    else if (issue.Contains(
                        "missing from the invoice",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfNew(
                            DiscrepancyType.MissingItems,
                            DiscrepancySeverity.High,
                            $"Missing invoice item: {item.PurchaseOrderItemName}",
                            issue,
                            item.PurchaseOrderItemName,
                            null,
                            "Confirm whether this purchase order item was delivered and invoiced.");
                    }
                    else if (issue.Contains(
                        "does not match any purchase order item",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfNew(
                            DiscrepancyType.Other,
                            DiscrepancySeverity.High,
                            $"Unexpected invoice item: {item.InvoiceItemName}",
                            issue,
                            null,
                            item.InvoiceItemName,
                            "Verify whether this item was authorised in the purchase order.");
                    }
                    else if (issue.Contains(
                        "Multiple purchase order items",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        AddIfNew(
                            DiscrepancyType.Other,
                            DiscrepancySeverity.Medium,
                            $"Ambiguous item match: {item.InvoiceItemName}",
                            issue,
                            null,
                            item.InvoiceItemName,
                            "Manually identify the correct purchase order item.");
                    }
                }
            }

            // 4. Save only when new discrepancies exist.
            if (newDiscrepancies.Count > 0)
            {
                await _discrepancyRepository.AddRangeAsync(
                    newDiscrepancies,
                    cancellationToken);

                await _discrepancyRepository.SaveChangesAsync(
                    cancellationToken);
            }
        }

    }
}
