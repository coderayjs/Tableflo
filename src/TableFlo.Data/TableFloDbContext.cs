using Microsoft.EntityFrameworkCore;
using TableFlo.Core.Models;

namespace TableFlo.Data;

/// <summary>
/// Entity Framework database context for TableFlo
/// </summary>
public class TableFloDbContext : DbContext
{
    public TableFloDbContext(DbContextOptions<TableFloDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Dealer> Dealers => Set<Dealer>();
    public DbSet<DealerCertification> DealerCertifications => Set<DealerCertification>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<BreakRecord> BreakRecords => Set<BreakRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Shift> Shifts => Set<Shift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });

        // Dealer configuration
        modelBuilder.Entity<Dealer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.Employee)
                  .WithMany()
                  .HasForeignKey(d => d.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(d => d.Certifications)
                  .WithOne(c => c.Dealer)
                  .HasForeignKey(c => c.DealerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(d => d.AssignmentHistory)
                  .WithOne(a => a.Dealer)
                  .HasForeignKey(a => a.DealerId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DealerCertification configuration
        modelBuilder.Entity<DealerCertification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameType).HasConversion<string>();
            entity.Property(e => e.ProficiencyLevel).HasConversion<string>();
            entity.Property(e => e.CrapsRole).HasConversion<string>();
        });

        // Table configuration
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TableNumber).IsUnique();
            entity.Property(e => e.TableNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.GameType).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Pit).HasMaxLength(50);
            entity.Property(e => e.MinBet).HasPrecision(18, 2);
            entity.Property(e => e.MaxBet).HasPrecision(18, 2);
            
            entity.HasMany(t => t.CurrentAssignments)
                  .WithOne(a => a.Table)
                  .HasForeignKey(a => a.TableId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Assignment configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CrapsRole).HasConversion<string>();
        });

        // BreakRecord configuration
        modelBuilder.Entity<BreakRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(b => b.Dealer)
                  .WithMany()
                  .HasForeignKey(b => b.DealerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(b => b.BreakType).IsRequired().HasMaxLength(20);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(a => a.Employee)
                  .WithMany()
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(a => a.ActionType).HasConversion<string>();
            entity.Property(a => a.Description).IsRequired().HasMaxLength(500);
            entity.Property(a => a.RelatedEntityType).HasMaxLength(100);
            entity.HasIndex(a => a.Timestamp);
        });

        // Shift configuration
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShiftName).IsRequired().HasMaxLength(100);
        });
    }
}

