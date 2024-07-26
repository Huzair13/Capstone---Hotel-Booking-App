using HotelServices.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelServices.Contexts
{
    public class HotelServicesContext : DbContext
    {
        public HotelServicesContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<HotelImage> HotelImages { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<HotelAmenity> HotelAmenities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            modelBuilder.Entity<Room>()
                .Property(b => b.Rent)
                .HasColumnType("decimal(18, 2)");


            modelBuilder.Entity<Hotel>()
                .Property(b => b.AverageRatings)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<HotelImage>()
                .HasOne(q => q.Hotel)
                .WithMany(q => q.HotelImages)
                .HasForeignKey(q => q.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<Room>()
                .HasOne(q => q.Hotel)
                .WithMany(q => q.Rooms)
                .HasForeignKey(q => q.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<HotelAmenity>()
                .HasOne(ra => ra.Hotel)
                .WithMany(r => r.HotelAmenities)
                .HasForeignKey(ra => ra.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<HotelAmenity>()
                .HasOne(ra => ra.Amenity)
                .WithMany(a => a.HotelAmenities)
                .HasForeignKey(ra => ra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

        }
    }
}
