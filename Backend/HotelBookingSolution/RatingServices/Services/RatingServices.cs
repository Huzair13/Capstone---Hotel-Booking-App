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
using RatingServices.Exceptions;
using System.Net.Http.Headers;
using System.Net.Http;

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

                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Check if the hotel exists
                var hotelExists = await CheckHotelExistsAsync(ratingDTO.HotelId,token);
                if (!hotelExists)
                {
                    throw new Exception("Hotel does not exist.");
                }

                // Check if the user exists
                var userExists = await CheckUserExistsAsync(userId,token);
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

                await UpdateHotelAverageRatingAsync(ratingDTO.HotelId,token);
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
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            try
            {
                // Check if the hotel exists
                var hotelExists = await CheckHotelExistsAsync(ratingDTO.HotelId,token);
                if (!hotelExists)
                {
                    throw new Exception("Hotel does not exist.");
                }

                var rating = await _ratingRepository.Get(ratingId);

                if (rating == null)
                {
                    throw new Exception("Rating not found or user does not own this rating.");
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
                await UpdateHotelAverageRatingAsync(updatedRating.HotelId,token);
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
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

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

                await UpdateHotelAverageRatingAsync(rating.HotelId,token);
                var deletedRating = await _ratingRepository.Delete(ratingId);

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
                IEnumerable<Rating> rating = new List<Rating>();
                try
                {
                    rating = await _ratingRepository.Get();
                }
                catch(NoSuchRatingException ex)
                {

                }
                return rating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all ratings");
                throw ex ;
            }
        }

        private async Task UpdateHotelAverageRatingAsync(int hotelId, string token)
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
                hotelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

        public async Task<bool> CheckHotelExistsAsync(int hotelId,string token)
        {
            var httpClient = _httpClientFactory.CreateClient("HotelService");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"api/GetHotelByID/{hotelId}");

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckUserExistsAsync(int userId,string token)
        {

            var httpClient = _httpClientFactory.CreateClient("UserAuthService");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"getuser/{userId}");

            return response.IsSuccessStatusCode;
        }
    }
}
