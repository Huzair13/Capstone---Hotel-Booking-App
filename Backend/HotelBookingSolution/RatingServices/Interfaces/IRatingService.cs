using RatingServices.Models.DTOs;
using RatingServices.Models;

namespace RatingServices.Interfaces
{
    public interface IRatingService
    {
        Task<Rating> AddRatingAsync(RatingInputDTO ratingDTO, int userId);
        Task<Rating> DeleteRatingAsync(int ratingId, int userId);
        Task<Rating> UpdateRatingAsync(int ratingId, RatingUpdateDTO ratingDTO, int userId);
        Task<Rating> GetRatingByIdAsync(int ratingId);
        Task<IEnumerable<Rating>> GetAllRatingsAsync();

        public Task<IEnumerable<Rating>> GetRatingsByHotelIdAsync(int hotelId);
    }

}
