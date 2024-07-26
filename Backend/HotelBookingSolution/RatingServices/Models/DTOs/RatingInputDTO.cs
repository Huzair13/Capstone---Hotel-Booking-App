namespace RatingServices.Models.DTOs
{
    public class RatingInputDTO
    {
        public int HotelId { get; set; }
        public string Feedback { get; set; }
        public decimal RatingValue { get; set; }
    }
}
