using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RatingServices.Models;

namespace RatingServices.Contexts
{
    public class RatingContext :DbContext
    {
        public DbSet<Rating> Reviews { get; set; }

        public RatingContext(DbContextOptions options) : base(options)
        {

        }
    }
}
