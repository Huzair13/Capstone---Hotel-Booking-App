namespace HotelServices.Exceptions
{
    public class NoSuchHotelRoomException :Exception
    {
        string ExceptionMessage;
        public NoSuchHotelRoomException()
        {
            ExceptionMessage = "Hotel Room Not Found";
        }
        public NoSuchHotelRoomException(int hotelID)
        {
            ExceptionMessage = $"Hotel Room with the HotelID : {hotelID} not found";
        }
        public NoSuchHotelRoomException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
