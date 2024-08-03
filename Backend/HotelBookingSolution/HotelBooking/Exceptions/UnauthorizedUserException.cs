using System.Runtime.Serialization;

namespace HotelBooking.Exceptions
{
    public class UnauthorizedUserException : Exception
    {
        string exceptionMessage;
        public UnauthorizedUserException(int id)
        {
            exceptionMessage = $"Invalid username or password";
        }
        public UnauthorizedUserException()
        {
            exceptionMessage = "Invalid username or password";
        }
        public UnauthorizedUserException(string message)
        {
            exceptionMessage = message;
        }
        public override string Message => exceptionMessage;
    }
}