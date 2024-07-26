namespace RatingServices.Models.DTOs
{
    public class RatingUpdateDTO
    {
        public int HotelId { get; set; }
        public string? Feedback { get; set; }
        public decimal? RatingValue { get; set; } 
    }
}
