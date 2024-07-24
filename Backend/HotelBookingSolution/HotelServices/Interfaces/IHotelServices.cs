using HotelServices.Models;
using HotelServices.Models.DTOs;

namespace HotelServices.Interfaces
{
    public interface IHotelServices
    {
        public Task<HotelReturnDTO> AddHotelAsync(HotelDTO hotelDTO);
        //public Task<HotelReturnDTO> EditQuizByIDAsync(QuizUpdateDTO quizDTO);
        //public Task<QuizReturnDTO> DeleteQuizByIDAsync(int quizID, int userId);
        public Task<List<HotelReturnDTO>> GetAllHotels();
        public Task<HotelReturnDTO> GetHotelByName(string hotelName);
        public Task<HotelReturnDTO> GetHotelByID(int hotelId);
        public Task<HotelReturnDTO> AddRoomToHotelAsync(int hotelId, RoomDTO roomDTO);

        public Task<List<RoomDTO>> GetAvailableRoomsAsync(DateTime checkInDate, DateTime checkOutDate, int numberOfGuests);



    }
}
