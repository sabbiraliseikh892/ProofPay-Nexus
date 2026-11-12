using ProofPayNexus.Application.DTOs.Evidence;
using ProofPayNexus.Application.Interfaces.Repositories;
using ProofPayNexus.Application.Interfaces.Services;
using ProofPayNexus.Application.Interfaces.Storage;
using ProofPayNexus.Domain.Entities;
using ProofPayNexus.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Application.Services
{
    public class EvidenceDocumentService : IEvidenceDocumentService
    {
        private readonly IEvidenceDocumentRepository _evidenceRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IFileStorageService _fileStorage;

        private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
            };

        public EvidenceDocumentService(
            IEvidenceDocumentRepository evidenceRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IFileStorageService fileStorage)
        {
            _evidenceRepository = evidenceRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _fileStorage = fileStorage;
        }

        // Upload evidence document
        public async Task<EvidenceDocumentResponse> UploadAsync(
            UploadEvidenceRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.PurchaseOrderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Purchase order ID is required.");
            }

            if (string.IsNullOrWhiteSpace(request.FileName))
            {
                throw new ArgumentException(
                    "File name is required.");
            }

            if (request.FileStream == null ||
                request.FileStream == Stream.Null ||
                !request.FileStream.CanRead)
            {
                throw new ArgumentException(
                    "A valid file stream is required.");
            }

            var extension = Path.GetExtension(request.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "File type is not supported. Allowed formats: PDF, JPG, JPEG and PNG.");
            }

            if (!Enum.IsDefined(
                    typeof(EvidenceDocumentType),
                    request.DocumentType))
            {
                throw new ArgumentException(
                    "Invalid evidence document type.");
            }

            // Validate the actual file content and size.
            var fileValidation = await ValidateFileAsync(
                request,
                cancellationToken);

            // Verify that the associated purchase order exists.
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
                request.PurchaseOrderId,
                cancellationToken);

            if (purchaseOrder == null)
            {
                throw new KeyNotFoundException(
                    "Purchase order not found.");
            }

            // Save the physical file.
            var storedFileName = await _fileStorage.SaveFileAsync(
                request.FileStream,
                Path.GetFileName(request.FileName),
                cancellationToken);

            try
            {
                var evidenceDocument = new EvidenceDocument
                {
                    Id = Guid.NewGuid(),

                    PurchaseOrderId = request.PurchaseOrderId,

                    FileName = Path.GetFileName(request.FileName),

                    StoredFileName = storedFileName,

                    FilePath = $"Evidence/{storedFileName}",

                    ContentType = fileValidation.ContentType,

                    FileSizeInBytes = fileValidation.FileSize,

                    DocumentType = request.DocumentType,

                    UploadedBy = request.UploadedBy,

                    UploadedAtUtc = DateTime.UtcNow
                };

                await _evidenceRepository.AddAsync(
                    evidenceDocument,
                    cancellationToken);

                await _evidenceRepository.SaveChangesAsync(
                    cancellationToken);

                return MapToResponse(evidenceDocument);
            }
            catch
            {
                // Remove the physical file if database saving fails.
                try
                {
                    await _fileStorage.DeleteFileAsync(
                        storedFileName,
                        CancellationToken.None);
                }
                catch
                {
                    // Preserve the original exception.
                }

                throw;
            }
        }

        // Get all evidence documents for a purchase order.
        public async Task<List<EvidenceDocumentResponse>>
            GetByPurchaseOrderIdAsync(
                Guid purchaseOrderId,
                CancellationToken cancellationToken = default)
        {
            if (purchaseOrderId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Purchase order ID is required.");
            }

            var documents =
                await _evidenceRepository.GetByPurchaseOrderIdAsync(
                    purchaseOrderId,
                    cancellationToken);

            return documents
                .Select(MapToResponse)
                .ToList();
        }

        // Get evidence document by ID.
        public async Task<EvidenceDocumentResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Evidence document ID is required.");
            }

            var document = await _evidenceRepository.GetByIdAsync(
                id,
                cancellationToken);

            if (document == null)
            {
                return null;
            }

            return MapToResponse(document);
        }

        // Validate file size, signature and extension.
        private static async Task<(long FileSize, string ContentType)>
            ValidateFileAsync(
                UploadEvidenceRequest request,
                CancellationToken cancellationToken)
        {
            var stream = request.FileStream;

            if (!stream.CanSeek)
            {
                throw new ArgumentException(
                    "File stream must support seeking.");
            }

            if (stream.Length <= 0 ||
                stream.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException(
                    "File size must be between 1 byte and 10 MB.");
            }

            var actualSize = stream.Length;

            if (actualSize != request.FileSizeInBytes)
            {
                throw new ArgumentException(
                    "Uploaded file size does not match the actual file size.");
            }

            var originalPosition = stream.Position;

            var header = new byte[8];

            try
            {
                stream.Position = 0;

                var bytesRead = await stream.ReadAsync(
                    header.AsMemory(0, header.Length),
                    cancellationToken);

                string contentType;

                // PDF signature: %PDF-
                if (bytesRead >= 5 &&
                    header[0] == 0x25 &&
                    header[1] == 0x50 &&
                    header[2] == 0x44 &&
                    header[3] == 0x46 &&
                    header[4] == 0x2D)
                {
                    contentType = "application/pdf";
                }
                // JPEG signature: FF D8 FF
                else if (bytesRead >= 3 &&
                         header[0] == 0xFF &&
                         header[1] == 0xD8 &&
                         header[2] == 0xFF)
                {
                    contentType = "image/jpeg";
                }
                // PNG signature
                else if (bytesRead >= 8 &&
                         header[0] == 0x89 &&
                         header[1] == 0x50 &&
                         header[2] == 0x4E &&
                         header[3] == 0x47 &&
                         header[4] == 0x0D &&
                         header[5] == 0x0A &&
                         header[6] == 0x1A &&
                         header[7] == 0x0A)
                {
                    contentType = "image/png";
                }
                else
                {
                    throw new ArgumentException(
                        "File content is invalid or unsupported.");
                }

                var extension = Path.GetExtension(request.FileName);

                var validExtension =
                    (contentType == "application/pdf" &&
                     extension.Equals(
                         ".pdf",
                         StringComparison.OrdinalIgnoreCase))
                    ||
                    (contentType == "image/jpeg" &&
                     (extension.Equals(
                          ".jpg",
                          StringComparison.OrdinalIgnoreCase)
                      ||
                      extension.Equals(
                          ".jpeg",
                          StringComparison.OrdinalIgnoreCase)))
                    ||
                    (contentType == "image/png" &&
                     extension.Equals(
                         ".png",
                         StringComparison.OrdinalIgnoreCase));

                if (!validExtension)
                {
                    throw new ArgumentException(
                        "File extension does not match its actual content.");
                }

                return (actualSize, contentType);
            }
            finally
            {
                // Restore the original stream position.
                stream.Position = originalPosition;
            }
        }

        // Map entity to response DTO.
        private static EvidenceDocumentResponse MapToResponse(
            EvidenceDocument document)
        {
            return new EvidenceDocumentResponse
            {
                Id = document.Id,

                PurchaseOrderId = document.PurchaseOrderId,

                FileName = document.FileName,

                ContentType = document.ContentType,

                FileSizeInBytes = document.FileSizeInBytes,

                DocumentType = document.DocumentType,

                UploadedBy = document.UploadedBy,

                UploadedAtUtc = document.UploadedAtUtc
            };
        }
    }
}
