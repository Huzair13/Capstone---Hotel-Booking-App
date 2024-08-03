namespace CancellationService.Models.DTOs
{
    public class CancelBookingDTO
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; } = false;
        public bool IsCancelled { get; set; } = false;
    }
}
