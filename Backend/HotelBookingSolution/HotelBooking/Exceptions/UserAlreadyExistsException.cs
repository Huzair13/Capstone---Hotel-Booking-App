namespace HotelBooking.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        string exceptionMessage;
        public UserAlreadyExistsException()
        {
            exceptionMessage = "User with the same EmailID or Mobile Number is already Registered. Please try to login";
        }

        public UserAlreadyExistsException(string Message)
        {
            exceptionMessage = Message;
        }
        public override string Message => exceptionMessage;
    }
}
