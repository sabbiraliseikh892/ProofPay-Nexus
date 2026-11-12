using ProofPayNexus.Application.DTOs.PurchaseOrders;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;
        public PurchaseOrderService(
    IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }
        public async Task<PurchaseOrderResponse> CreateAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.PurchaseOrderNumber))
                throw new ArgumentException("Purchase order number is required.");

            if (string.IsNullOrWhiteSpace(request.SupplierName))
                throw new ArgumentException("Supplier name is required.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) ||
                request.CurrencyCode.Trim().Length != 3)
            {
                throw new ArgumentException(
                    "Currency code must contain exactly three characters.");
            }

            if (request.OrderDate == default)
                throw new ArgumentException("Order date is required.");

            if (request.ExpectedDeliveryDate.HasValue &&
                request.ExpectedDeliveryDate.Value < request.OrderDate)
            {
                throw new ArgumentException(
                    "Expected delivery date cannot be earlier than order date.");
            }

            if (request.Items == null || request.Items.Count == 0)
                throw new ArgumentException(
                    "At least one purchase order item is required.");

            var existingOrder =
                await _repository.GetByPurchaseOrderNumberAsync(
                    request.PurchaseOrderNumber.Trim(),
                    cancellationToken);

            if (existingOrder != null)
                throw new InvalidOperationException(
                    "A purchase order with this number already exists.");

            foreach (var item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName))
                    throw new ArgumentException("Item name is required.");

                if (item.Quantity <= 0)
                    throw new ArgumentException(
                        "Item quantity must be greater than zero.");

                if (item.UnitPrice <= 0)
                    throw new ArgumentException(
                        "Item unit price must be greater than zero.");
            }

            var currencyCode = request.CurrencyCode.Trim().ToUpperInvariant();

            var totalAmount = request.Items.Sum(
                item => item.Quantity * item.UnitPrice);

            var purchaseOrder = new PurchaseOrder
            {
                Id = Guid.NewGuid(),
                PurchaseOrderNumber = request.PurchaseOrderNumber.Trim(),
                SupplierName = request.SupplierName.Trim(),
                CurrencyCode = currencyCode,
                TotalAmount = totalAmount,
                OrderDate = request.OrderDate,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                Status = PurchaseOrderStatus.Draft,
                CreatedAtUtc = DateTime.UtcNow,

                Items = request.Items.Select(item =>
                    new PurchaseOrderItem
                    {
                        Id = Guid.NewGuid(),
                        ItemName = item.ItemName.Trim(),
                        SKU = item.SKU?.Trim(),
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    }).ToList()
            };

            await _repository.AddAsync(
                purchaseOrder,
                cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);

            return MapToResponse(purchaseOrder);
        }

        public async Task<PurchaseOrderResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var purchaseOrder = await _repository.GetByIdAsync(
                id,
                cancellationToken);

            return purchaseOrder == null
                ? null
                : MapToResponse(purchaseOrder);
        }

        public async Task<List<PurchaseOrderResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var purchaseOrders = await _repository.GetAllAsync(
                cancellationToken);

            return purchaseOrders
                .Select(MapToResponse)
                .ToList();
        }

        private static PurchaseOrderResponse MapToResponse(
            PurchaseOrder purchaseOrder)
        {
            return new PurchaseOrderResponse
            {
                Id = purchaseOrder.Id,
                PurchaseOrderNumber = purchaseOrder.PurchaseOrderNumber,
                SupplierName = purchaseOrder.SupplierName,
                CurrencyCode = purchaseOrder.CurrencyCode,
                TotalAmount = purchaseOrder.TotalAmount,
                OrderDate = purchaseOrder.OrderDate,
                ExpectedDeliveryDate = purchaseOrder.ExpectedDeliveryDate,
                Status = purchaseOrder.Status,
                CreatedAtUtc = purchaseOrder.CreatedAtUtc,

                Items = purchaseOrder.Items.Select(item =>
                    new PurchaseOrderItemResponse
                    {
                        Id = item.Id,
                        ItemName = item.ItemName,
                        SKU = item.SKU,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        LineTotal = item.LineTotal
                    }).ToList()
            };
        }
    }
}
