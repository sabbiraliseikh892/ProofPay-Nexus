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
    public class EvidenceAnalysisConfiguration
    : IEntityTypeConfiguration<EvidenceAnalysis>
    {
        public void Configure(EntityTypeBuilder<EvidenceAnalysis> builder)
        {
            builder.ToTable("EvidenceAnalyses");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ModelName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ExtractedDataJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.AnalysisSummary)
                .HasMaxLength(4000);

            builder.Property(x => x.ConfidenceScore)
                .HasPrecision(5, 2);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.EvidenceDocumentId);

            builder.HasIndex(x => x.CreatedAtUtc);

            builder.HasOne(x => x.EvidenceDocument)
                .WithMany(x => x.Analyses)
                .HasForeignKey(x => x.EvidenceDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
