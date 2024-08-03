using HotelServices.Models;
using System.Runtime.Serialization;

namespace HotelServices.Exceptions
{
    public class NoSuchAmenityFound : Exception
    {
        string ExceptionMessage;
        public NoSuchAmenityFound()
        {
            ExceptionMessage = "Amenity Not Found";
        }
        public NoSuchAmenityFound(int amenityID)
        {
            ExceptionMessage = $"Amenity with the AmenityID : {amenityID} not found";
        }
        public NoSuchAmenityFound(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}