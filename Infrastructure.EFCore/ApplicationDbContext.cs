using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.EFCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<Paycheck> Paychecks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.SKU).HasMaxLength(50);
            entity.HasIndex(e => e.SKU).IsUnique();
        });

        // PaymentMethod configuration
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Receipt configuration
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Receipts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.PaymentMethod)
                .WithMany(p => p.Receipts)
                .HasForeignKey(e => e.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ReceiptItem configuration
        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Receipt)
                .WithMany(r => r.Items)
                .HasForeignKey(e => e.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.HourlyRate).HasPrecision(18, 2);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // TimeEntry configuration
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HoursWorked).HasPrecision(5, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            
            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.TimeEntries)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Paycheck configuration
        modelBuilder.Entity<Paycheck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GrossPay).HasPrecision(18, 2);
            entity.Property(e => e.NetPay).HasPrecision(18, 2);
            entity.Property(e => e.TaxDeduction).HasPrecision(18, 2);
            entity.Property(e => e.OtherDeductions).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.Paychecks)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        SeedData(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback configuration for development/testing
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KioskDb;Trusted_Connection=true;MultipleActiveResultSets=true");
        }

        // Enable sensitive data logging in development
        #if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
        #endif
    }


    private void SeedData(ModelBuilder modelBuilder)
    {
        // TODO: Add seed data here
        // Example:
        // modelBuilder.Entity<YourEntity>().HasData(
        //     new YourEntity { Id = 1, Name = "Sample Entity 1" },
        //     new YourEntity { Id = 2, Name = "Sample Entity 2" }
        // );
    }
}