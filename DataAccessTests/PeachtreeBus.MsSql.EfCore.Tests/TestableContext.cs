using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.MsSql.EfCore.Tests;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Code")]
public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }
    public DateTime Occured { get; init; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Message { get; init; } = string.Empty;
}

public class TestableContext(
    ISqlSharedDatabase sharedDatabase) 
    : MsSqlSharedDatabaseDbContext(sharedDatabase)
{ 
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(auditLog =>
        {
            auditLog.ToTable("AuditLog", "ExampleApp");
            auditLog.HasKey(a => a.Id);
        });
    }
}
