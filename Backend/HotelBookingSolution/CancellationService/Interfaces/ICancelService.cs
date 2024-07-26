using CancellationService.Models;
using CancellationService.Models.DTOs;

namespace CancellationService.Interfaces
{
    public interface ICancelService
    {
        public Task<CancelReturnDTO> CancelTheBooking(int BookingId,int currUserID);
    }
}
