using System.Runtime.Serialization;

namespace RatingServices.Exceptions
{
    [Serializable]
    public class NoSuchRatingException : Exception
    {
        private int ratingID;

        public NoSuchRatingException()
        {
        }

        public NoSuchRatingException(int ratingID)
        {
            this.ratingID = ratingID;
        }

        public NoSuchRatingException(string? message) : base(message)
        {
        }

        public NoSuchRatingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoSuchRatingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}