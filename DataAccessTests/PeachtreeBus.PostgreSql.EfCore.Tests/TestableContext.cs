using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PeachtreeBus.DatabaseSharing;

namespace PeachtreeBus.PostgreSql.EfCore.Tests;

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
    INpgSqlSharedDatabase sharedDatabase) 
    : PostgreSqlSharedDatabaseDbContext(sharedDatabase)
{ 
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(auditLog =>
        {
            auditLog.ToTable("audit_log", "example_app");
            auditLog.HasKey(a => a.Id);
            
            // In a real project it is probably easier to use EFCore.NamingConventions
            // to make EF Automatically generate postgres style names from C# style names.
            // https://www.npgsql.org/efcore/modeling/tables.html
            // Since this is just small test code, that dependency can be avoided by
            // manually specifying column names.
            auditLog.Property(a => a.Id).HasColumnName("id");
            auditLog.Property(a => a.Occured).HasColumnName("occured");
            auditLog.Property(a => a.Message).HasColumnName("message");
        });
    }
}
