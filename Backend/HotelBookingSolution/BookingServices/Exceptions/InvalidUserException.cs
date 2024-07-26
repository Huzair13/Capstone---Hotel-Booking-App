namespace CancellationService.Exceptions
{
    public class InvalidUserException :Exception
    {
        string ExceptionMessage;
        public InvalidUserException()
        {
            ExceptionMessage = "User is Invalid";
        }
        public InvalidUserException(int userID)
        {
            ExceptionMessage = $"User with the User ID : {userID} Is Invalid";
        }
        public InvalidUserException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;

    }
}
