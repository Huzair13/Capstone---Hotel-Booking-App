using HotelServices.Models;
using System.Runtime.Serialization;

namespace HotelServices.Exceptions
{
    [Serializable]
    internal class NoSuchAmenityFound : Exception
    {
        private Amenity? amenity;

        public NoSuchAmenityFound()
        {
        }

        public NoSuchAmenityFound(int amenityId)
        {
        }

        public NoSuchAmenityFound(Amenity? amenity)
        {
            this.amenity = amenity;
        }

        public NoSuchAmenityFound(string? message) : base(message)
        {
        }

        public NoSuchAmenityFound(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoSuchAmenityFound(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}