using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HotelBooking.Contexts
{
    public class HotelBookingDbContext : DbContext
    {
        public HotelBookingDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserDetails> UsersDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 101,
                    Name = "Janu",
                    Email = "janu@gmail.com",
                    MobileNumber = "1234567890",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = Gender.Women,
                    UserType = "Guest"
                }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 102,
                    Name = "Saren",
                    Email = "sarem@gmail.com",
                    MobileNumber = "9877665544",
                    DateOfBirth = new DateTime(1999, 1, 1),
                    Gender = Gender.Men,
                    UserType = "Guest"
                }
            );
        }
    }
}
