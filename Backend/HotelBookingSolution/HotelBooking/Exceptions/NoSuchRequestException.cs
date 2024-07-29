namespace AuthenticationServices.Exceptions
{
    public class NoSuchRequestException : Exception
    {
        string exceptionMessage;
        public NoSuchRequestException(int id)
        {
            exceptionMessage = $"No Request with the given ID : {id}";
        }
        public NoSuchRequestException()
        {
            exceptionMessage = "No such Request found";
        }
        public NoSuchRequestException(string message)
        {
            exceptionMessage = message;
        }
        public override string Message => exceptionMessage;
    }
}
