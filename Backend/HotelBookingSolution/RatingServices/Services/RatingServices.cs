using RatingServices.Interfaces;
using RatingServices.Models.DTOs;
using RatingServices.Models;
using RatingServices.Contexts;
using HotelBooking.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RatingServices.Services
{
    public class RatingService : IRatingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RatingService> _logger;
        private readonly IRepository<int, Rating> _ratingRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RatingService(
            IHttpClientFactory httpClientFactory,
            ILogger<RatingService> logger,
            IHttpContextAccessor httpContextAccessor,
            IRepository<int, Rating> ratingRepo)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _ratingRepository = ratingRepo;
        }

        public async Task<Rating> AddRatingAsync(RatingInputDTO ratingDTO, int userId)
        {
            try
            {
                // Check if the hotel exists
                var hotelExists = await CheckHotelExistsAsync(ratingDTO.HotelId);
                if (!hotelExists)
                {
                    throw new Exception("Hotel does not exist.");
                }

                // Check if the user exists
                var userExists = await CheckUserExistsAsync(userId);
                if (!userExists)
                {
                    throw new Exception("User does not exist.");
                }

                // Check if the user has already rated this hotel
                var existingRating = (await _ratingRepository.Get())
                    .FirstOrDefault(r => r.HotelId == ratingDTO.HotelId && r.UserId == userId);

                if (existingRating != null)
                {
                    throw new Exception("User has already given a review for this hotel.");
                }

                // Add rating
                var rating = new Rating
                {
                    UserId = userId,
                    HotelId = ratingDTO.HotelId,
                    Feedback = ratingDTO.Feedback,
                    RatingValue = ratingDTO.RatingValue,
                    CreatedAt = DateTime.UtcNow
                };

                var addedRating = await _ratingRepository.Add(rating);

                await UpdateHotelAverageRatingAsync(ratingDTO.HotelId);
                return addedRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the rating");
                throw;
            }
        }

        public async Task<Rating> UpdateRatingAsync(int ratingId, RatingUpdateDTO ratingDTO, int userId)
        {
            try
            {
                var rating = await _ratingRepository.Get(ratingId);

                if (rating == null)
                {
                    throw new Exception("Rating not found or user does not own this rating.");
                }

                // Check if the hotel exists
                var hotelExists = await CheckHotelExistsAsync(ratingDTO.HotelId);
                if (!hotelExists)
                {
                    throw new Exception("Hotel does not exist.");
                }

                // Update rating only if the new value is provided
                if (!string.IsNullOrWhiteSpace(ratingDTO.Feedback))
                {
                    rating.Feedback = ratingDTO.Feedback;
                }

                if (ratingDTO.RatingValue.HasValue)
                {
                    rating.RatingValue = ratingDTO.RatingValue.Value;
                }

                // Optionally update timestamp
                rating.CreatedAt = DateTime.Now;
                var updatedRating = await _ratingRepository.Update(rating);

                // Update hotel average rating
                await UpdateHotelAverageRatingAsync(rating.HotelId);
                return updatedRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the rating");
                throw;
            }
        }

        public async Task<Rating> DeleteRatingAsync(int ratingId, int userId)
        {
            try
            {
                var rating = await _ratingRepository.Get(ratingId);

                if (rating == null)
                {
                    throw new Exception("Rating not found.");
                }

                if (rating.UserId != userId)
                {
                    throw new Exception("User does not own this rating.");
                }

                var deletedRating = await _ratingRepository.Delete(ratingId);

                // Update hotel average rating
                await UpdateHotelAverageRatingAsync(rating.HotelId);
                return deletedRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the rating");
                throw;
            }
        }

        public async Task<IEnumerable<Rating>> GetRatingsByHotelIdAsync(int hotelId)
        {
            try
            {
                // Fetch all ratings
                var allRatings = await _ratingRepository.Get();

                // Filter ratings by hotelId
                var filteredRatings = allRatings.Where(r => r.HotelId == hotelId);

                return filteredRatings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting ratings for hotel ID {HotelId}", hotelId);
                throw;
            }
        }

        public async Task<Rating> GetRatingByIdAsync(int ratingId)
        {
            try
            {
                var rating = await _ratingRepository.Get(ratingId);

                if (rating == null)
                {
                    throw new Exception("Rating not found.");
                }

                return rating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the rating");
                throw;
            }
        }

        public async Task<IEnumerable<Rating>> GetAllRatingsAsync()
        {
            try
            {
                return await _ratingRepository.Get();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all ratings");
                throw;
            }
        }

        private async Task UpdateHotelAverageRatingAsync(int hotelId)
        {
            try
            {
                // Fetch all ratings for the hotel
                var ratings = await GetRatingsByHotelIdAsync(hotelId);

                decimal averageRatingDecimal;
                if (ratings == null || !ratings.Any())
                {
                    averageRatingDecimal = 0;
                }
                else
                {
                    // Calculate the new average rating
                    var averageRating = ratings.Average(r => r.RatingValue);

                    // Ensure the average rating is a decimal value
                    averageRatingDecimal = (decimal)averageRating;
                }

                // Update the hotel average rating in the Hotel microservice
                var hotelClient = _httpClientFactory.CreateClient("HotelService");
                var updateHotelRatingDTO = new
                {
                    HotelId = hotelId,
                    AverageRating = averageRatingDecimal
                };

                var response = await hotelClient.PutAsJsonAsync("api/UpdateAverageRatings", updateHotelRatingDTO);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to update the hotel's average rating.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the hotel's average rating");
                throw;
            }
        }

        private async Task<bool> CheckHotelExistsAsync(int hotelId)
        {
            var httpClient = _httpClientFactory.CreateClient("HotelService");
            var response = await httpClient.GetAsync($"api/GetHotelByID/{hotelId}");

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> CheckUserExistsAsync(int userId)
        {
            var httpClient = _httpClientFactory.CreateClient("UserAuthService");
            var response = await httpClient.GetAsync($"getuser/{userId}");

            return response.IsSuccessStatusCode;
        }
    }
}
