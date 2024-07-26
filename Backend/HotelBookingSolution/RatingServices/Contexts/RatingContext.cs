using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RatingServices.Models;
using System.Reflection.Emit;

namespace RatingServices.Contexts
{
    public class RatingContext :DbContext
    {
        public DbSet<Rating> Reviews { get; set; }

        public RatingContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>()
                .Property(b => b.RatingValue)
                .HasColumnType("decimal(18, 2)");
        }
    }
}
