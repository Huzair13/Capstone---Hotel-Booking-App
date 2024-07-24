namespace HotelServices.Exceptions
{
    public class NoSuchHotelException :Exception
    {
        string ExceptionMessage;
        public NoSuchHotelException()
        {
            ExceptionMessage = "Hotel Not Found";
        }
        public NoSuchHotelException(int hotelID)
        {
            ExceptionMessage = $"Hotel with the HotelID : {hotelID} not found";
        }
        public NoSuchHotelException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
