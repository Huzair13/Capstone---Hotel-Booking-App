using BookingServices.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingServices.Contexts
{
    public class HotelBookingContext :DbContext
    {
        public HotelBookingContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(18, 2)");
            modelBuilder.Entity<BookingDetail>()
                .Property(b => b.Rent)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Booking>()
                 .HasMany(b => b.BookingDetails)
                 .WithOne(bd => bd.Booking)
                 .HasForeignKey(bd => bd.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
