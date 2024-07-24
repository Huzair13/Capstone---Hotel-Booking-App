namespace HotelBooking.Exceptions
{
    public class NoSuchUserException : Exception
    {
        string exceptionMessage;
        public NoSuchUserException(int id)
        {
            exceptionMessage = $"No user with the given ID : {id}";
        }
        public NoSuchUserException()
        {
            exceptionMessage = "No such user found";
        }
        public NoSuchUserException(string message)
        {
            exceptionMessage = message;
        }
        public override string Message => exceptionMessage;
    }
}
