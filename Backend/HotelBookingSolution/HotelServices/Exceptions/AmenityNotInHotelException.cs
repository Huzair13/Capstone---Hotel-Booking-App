using System.Runtime.Serialization;

namespace HotelServices.Exceptions
{
    [Serializable]
    internal class AmenityNotInHotelException : Exception
    {
        private int roomId;
        private int amenityId;

        public AmenityNotInHotelException()
        {
        }

        public AmenityNotInHotelException(string? message) : base(message)
        {
        }

        public AmenityNotInHotelException(int roomId, int amenityId)
        {
            this.roomId = roomId;
            this.amenityId = amenityId;
        }

        public AmenityNotInHotelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AmenityNotInHotelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}