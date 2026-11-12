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
    public class AuditLogConfiguration
      : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.PerformedBy)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.MetadataJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45);

            builder.HasIndex(x => new
            {
                x.EntityType,
                x.EntityId
            });

            builder.HasIndex(x => x.CreatedAtUtc);

            builder.HasIndex(x => x.Action);
        }
    }
}
