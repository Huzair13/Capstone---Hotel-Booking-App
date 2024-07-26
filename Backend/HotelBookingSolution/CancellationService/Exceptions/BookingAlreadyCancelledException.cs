namespace CancellationService.Exceptions
{
    public class BookingAlreadyCancelledException :Exception
    {
        string ExceptionMessage;
        public BookingAlreadyCancelledException()
        {
            ExceptionMessage = "Booking Already cancelled";
        }
        public BookingAlreadyCancelledException(int bookingID)
        {
            ExceptionMessage = $"Booking with the BookingID : {bookingID} already cancelled";
        }
        public BookingAlreadyCancelledException(string message)
        {
            ExceptionMessage = message;
        }

        public override string Message => ExceptionMessage;
    }
}
