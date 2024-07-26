namespace BookingServices.Exceptions
{
    public class NoSuchRefundsException : Exception
    {
        string ExceptionMessage;
        public NoSuchRefundsException()
        {
            ExceptionMessage = "Refunds Not Found";
        }
        public NoSuchRefundsException(int refundID)
        {
            ExceptionMessage = $"Refund with the Refund ID : {refundID} not found";
        }
        public NoSuchRefundsException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
