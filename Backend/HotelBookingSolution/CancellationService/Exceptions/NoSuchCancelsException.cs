namespace BookingServices.Exceptions
{
    public class NoSuchCancelsException :Exception
    {
        string ExceptionMessage;
        public NoSuchCancelsException()
        {
            ExceptionMessage = "Cancellation Not Found";
        }
        public NoSuchCancelsException(int cancellationID)
        {
            ExceptionMessage = $"Cancellation with the Cancellation ID : {cancellationID} not found";
        }
        public NoSuchCancelsException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
