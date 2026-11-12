using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProofPayNexus.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProofPayNexus.Persistence.Configurations
{
    public class EvidenceDocumentConfiguration
    : IEntityTypeConfiguration<EvidenceDocument>
    {
        public void Configure(EntityTypeBuilder<EvidenceDocument> builder)
        {
            builder.ToTable("EvidenceDocuments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.FileSizeInBytes)
                .IsRequired();

            builder.Property(x => x.DocumentType)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.UploadedBy)
                .HasMaxLength(150);

            builder.HasIndex(x => x.PurchaseOrderId);

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Analyses)
                .WithOne(x => x.EvidenceDocument)
                .HasForeignKey(x => x.EvidenceDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
