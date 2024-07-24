namespace BookingServices.Exceptions
{
    public class NoSuchBookingException :Exception
    {
        string ExceptionMessage;
        public NoSuchBookingException()
        {
            ExceptionMessage = "Booking Not Found";
        }
        public NoSuchBookingException(int bookingID)
        {
            ExceptionMessage = $"Booking with the BookingID : {bookingID} not found";
        }
        public NoSuchBookingException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
