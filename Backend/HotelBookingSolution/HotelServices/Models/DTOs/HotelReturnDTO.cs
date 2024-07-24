namespace HotelServices.Models.DTOs
{
    public class HotelReturnDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public int NumOfRooms { get; set; }
        public int AverageRatings { get; set; }
        public string Description { get; set; }
        public List<string> HotelImages { get; set; }
        public List<int>? RoomIDs { get; set; }
    }
}
