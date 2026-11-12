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
    public class DiscrepancyConfiguration
    : IEntityTypeConfiguration<Discrepancy>
    {
        public void Configure(EntityTypeBuilder<Discrepancy> builder)
        {
            builder.ToTable("Discrepancies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Severity)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(x => x.ExpectedValue)
                .HasMaxLength(500);

            builder.Property(x => x.ActualValue)
                .HasMaxLength(500);

            builder.Property(x => x.RecommendedAction)
                .HasMaxLength(1000);

            builder.Property(x => x.ResolutionNotes)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.PurchaseOrderId);

            builder.HasIndex(x => new
            {
                x.PurchaseOrderId,
                x.Status,
                x.Severity
            });

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.Discrepancies)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.EvidenceDocument)
                .WithMany()
                .HasForeignKey(x => x.EvidenceDocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
