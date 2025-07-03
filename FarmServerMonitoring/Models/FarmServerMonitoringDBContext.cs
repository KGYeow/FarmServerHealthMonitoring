using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace FarmServerMonitoring.Models
{
    public partial class FarmServerMonitoringDBContext : DbContext
    {
        public FarmServerMonitoringDBContext()
        {
        }

        public FarmServerMonitoringDBContext(DbContextOptions<FarmServerMonitoringDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Collection> Collection { get; set; }
        public virtual DbSet<ConnectionBroker> ConnectionBroker { get; set; }
        public virtual DbSet<ConnectionBrokerServerHealthMap> ConnectionBrokerServerHealthMap { get; set; }
        public virtual DbSet<ServerHealthReport> ServerHealthReport { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=awase1pensql81;Database=FarmServerMonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Collection>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CdriveFreeSpace).HasColumnName("CDriveFreeSpace");

                entity.Property(e => e.DdriveFreeSpace).HasColumnName("DDriveFreeSpace");

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('No')");

                entity.Property(e => e.PendingReboot)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ReportId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Uptime)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Report)
                    .WithMany(p => p.Collection)
                    .HasForeignKey(d => d.ReportId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Collection_ServerHealthReport");
            });

            modelBuilder.Entity<ConnectionBroker>(entity =>
            {
                entity.HasKey(e => e.ServerName)
                    .HasName("PK_ConnectionBroker_1");

                entity.Property(e => e.ServerName).HasMaxLength(50);
            });

            modelBuilder.Entity<ConnectionBrokerServerHealthMap>(entity =>
            {
                entity.HasKey(e => new { e.ReportId, e.ConnectionBrokerName });

                entity.Property(e => e.ReportId).HasMaxLength(100);

                entity.Property(e => e.ConnectionBrokerName).HasMaxLength(50);

                entity.HasOne(d => d.ConnectionBrokerNameNavigation)
                    .WithMany(p => p.ConnectionBrokerServerHealthMap)
                    .HasForeignKey(d => d.ConnectionBrokerName)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ConnectionBrokerServerHealthMap_ConnectionBroker");

                entity.HasOne(d => d.Report)
                    .WithMany(p => p.ConnectionBrokerServerHealthMap)
                    .HasForeignKey(d => d.ReportId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ConnectionBrokerServerHealthMap_ServerHealthReport");
            });

            modelBuilder.Entity<ServerHealthReport>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(100);

                entity.Property(e => e.CdriveFreeSpaceAvg).HasColumnName("CDriveFreeSpaceAvg");

                entity.Property(e => e.CollectionName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DdriveFreeSpaceAvg).HasColumnName("DDriveFreeSpaceAvg");

                entity.Property(e => e.ReportName).IsRequired();

                entity.Property(e => e.ScriptEndTime).HasColumnType("datetime");

                entity.Property(e => e.ScriptStartTime).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
