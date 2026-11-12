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
    public class PaymentTransactionConfiguration
     : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionReference)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.TransactionReference)
                .IsUnique();

            builder.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(x => x.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.PayPalOrderId)
                .HasMaxLength(100);

            builder.Property(x => x.PayPalAuthorizationId)
                .HasMaxLength(100);

            builder.Property(x => x.PayPalCaptureId)
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ApprovalStatus)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ReviewedBy)
                .HasMaxLength(150);

            builder.Property(x => x.ReviewNotes)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.PurchaseOrderId);

            builder.HasIndex(x => x.PayPalOrderId);

            builder.HasIndex(x => x.PayPalAuthorizationId);

            builder.HasIndex(x => x.PayPalCaptureId);

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PaymentAssessment)
                .WithMany(x => x.PaymentTransactions)
                .HasForeignKey(x => x.PaymentAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
