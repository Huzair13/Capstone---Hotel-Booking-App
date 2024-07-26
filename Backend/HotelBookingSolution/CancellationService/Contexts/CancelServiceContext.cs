using CancellationService.Models;
using Microsoft.EntityFrameworkCore;

namespace CancellationService.Contexts
{
    public class CancelServiceContext :DbContext
    {
        public CancelServiceContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Cancel> Cancels { get; set; }
        public DbSet<Refund> Refunds { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Refund>()
                .Property(b => b.RefundAmount)
                .HasColumnType("decimal(18, 2)");
        }
    }
}
