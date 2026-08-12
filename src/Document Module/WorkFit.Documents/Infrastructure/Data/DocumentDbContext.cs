using WorkFit.Documents.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorkFit.Documents.Infrastructure.Data;

public sealed class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
        
    }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("document");
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.ContentType).IsRequired();
            entity.Property(e => e.UploadedBy).IsRequired();
            entity.Property(e => e.StorageKey).IsRequired();
            entity.Property(e => e.Size).IsRequired();
            entity.OwnsOne(d => d.AccessEntry).ToJson();
                

        });
    }
}
