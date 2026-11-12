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
    public class PaymentAssessmentConfiguration
    : IEntityTypeConfiguration<PaymentAssessment>
    {
        public void Configure(EntityTypeBuilder<PaymentAssessment> builder)
        {
            builder.ToTable("PaymentAssessments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RiskScore)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(x => x.Recommendation)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Explanation)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(x => x.RiskFactorsJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ModelName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.PurchaseOrderId);

            builder.HasIndex(x => x.AssessedAtUtc);

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.PaymentAssessments)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.PaymentTransactions)
                .WithOne(x => x.PaymentAssessment)
                .HasForeignKey(x => x.PaymentAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
