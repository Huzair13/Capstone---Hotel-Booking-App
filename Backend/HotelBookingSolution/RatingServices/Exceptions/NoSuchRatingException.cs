using System.Runtime.Serialization;

namespace RatingServices.Exceptions
{
    public class NoSuchRatingException : Exception
    {
        string ExceptionMessage;
        public NoSuchRatingException()
        {
            ExceptionMessage = "Rating Not Found";
        }
        public NoSuchRatingException(int ratingID)
        {
            ExceptionMessage = $"Rating with the RatingID : {ratingID} not found";
        }
        public NoSuchRatingException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}