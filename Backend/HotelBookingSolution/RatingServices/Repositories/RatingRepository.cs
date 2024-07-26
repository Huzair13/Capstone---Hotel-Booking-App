using HotelBooking.Interfaces;
using Microsoft.EntityFrameworkCore;
using RatingServices.Contexts;
using RatingServices.Exceptions;
using RatingServices.Models;

namespace RatingServices.Repositories
{
    public class RatingRepository : IRepository<int, Rating>
    {
        private readonly RatingContext _context;

        public RatingRepository(RatingContext context)
        {
            _context = context;
        }
        public async Task<Rating> Add(Rating item)
        {
            _context.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<Rating> Delete(int ratingID)
        {
            var rating = await Get(ratingID);
            _context.Remove(rating);
            await _context.SaveChangesAsync(true);
            return rating;
        }

        public async Task<Rating> Get(int ratingID)
        {
            var rating = await _context.Reviews
                        .FirstOrDefaultAsync(h => h.Id == ratingID);

            if (rating != null)
            {
                return rating;
            }
            throw new NoSuchRatingException(ratingID);
        }

        public async Task<IEnumerable<Rating>> Get()
        {
            var hotels = await _context.Reviews.ToListAsync();
            if (hotels.Count != 0)
            {
                return hotels;
            }
            throw new NoSuchRatingException();
        }

        public async Task<Rating> Update(Rating rating)
        {
            _context.Update(rating);
            await _context.SaveChangesAsync();
            var existingHotel = await Get(rating.Id);
            return existingHotel;
        }
    }
}
