namespace CancellationService.Models.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsPaid { get; set; }
    }
}
