namespace CancellationService.Exceptions
{
    public class UserNotActiveException :Exception
    {
        string ExceptionMessage;
        public UserNotActiveException()
        {
            ExceptionMessage = "User Account is Deactivated";
        }
        public UserNotActiveException(int userID)
        {
            ExceptionMessage = $"User with the User ID : {userID} is Deactivated";
        }
        public UserNotActiveException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
