using Microsoft.EntityFrameworkCore;
using TransferaShipments.Domain.Entities;

namespace TransferaShipments.Persistence.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shipment> Shipments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.ReferenceNumber).IsUnique();
            b.Property(x => x.ReferenceNumber).IsRequired();
            b.Property(x => x.Sender).IsRequired();
            b.Property(x => x.Recipient).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}