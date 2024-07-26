namespace RatingServices.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HotelId { get; set; }
        public string Feedback { get; set; }
        public decimal RatingValue { get; set; } 
        public DateTime CreatedAt { get; set; }
    }
}
