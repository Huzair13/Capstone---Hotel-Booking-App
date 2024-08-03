namespace HotelServices.Models.DTOs
{
    public class HotelUpdateDataDTO
    {
        public int HotelId { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
    }
}
