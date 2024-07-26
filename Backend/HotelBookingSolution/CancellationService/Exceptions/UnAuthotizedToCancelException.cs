namespace CancellationService.Exceptions
{
    public class UnAuthotizedToCancelException : Exception
    {
        string ExceptionMessage;
        public UnAuthotizedToCancelException()
        {
            ExceptionMessage = "Unauthorizes to cancel this Booking";
        }
        public UnAuthotizedToCancelException(int userID)
        {
            ExceptionMessage = $"User with the UserID : {userID} cannot cancel the booking";
        }
        public UnAuthotizedToCancelException(string message)
        {
            ExceptionMessage = message;
        }

        public override string Message => ExceptionMessage;
    }
}
