using Microsoft.EntityFrameworkCore;
using PeachtreeBus.DatabaseSharing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.EntityFrameworkCore.Tests;

[ExcludeFromCodeCoverage(Justification = "Non-Shipping Code")]
public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public virtual int Id { get; set; }
    public virtual DateTime Occured { get; set; }
    public virtual string Message { get; set; } = string.Empty;
}

public class TestableContext(
    ISharedDatabase sharedDatabase) 
    : SharedDatabaseDbContext(sharedDatabase)
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
