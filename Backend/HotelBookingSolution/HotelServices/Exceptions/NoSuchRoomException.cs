namespace HotelServices.Exceptions
{
    public class NoSuchRoomException :Exception
    {
        string ExceptionMessage;
        public NoSuchRoomException()
        {
            ExceptionMessage = "Room Not Found";
        }
        public NoSuchRoomException(int roomID)
        {
            ExceptionMessage = $"Room with the RoomID : {roomID} not found";
        }
        public NoSuchRoomException(string message)
        {
            ExceptionMessage = message;
        }
        public override string Message => ExceptionMessage;
    }
}
