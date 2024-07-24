using BookingServices.Models.DTOs;

namespace BookingServices.Interfaces
{
    public interface IBookingServices
    {
        public Task<BookingReturnDTO> AddBookingAsync(BookingDTO bookingDTO);
        public Task<List<BookingReturnDTO>> GetAllBookings();
        public Task<BookingReturnDTO> GetBookingByID(int bookingId);
        public Task<BookingReturnDTO> GetBookingByUser(int userId);
        public Task<List<int>> GetBookedRoomNumbersAsync(DateTime checkInDate, DateTime checkOutDate);
    }
}
