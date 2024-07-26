namespace CancellationService.Models.DTOs
{
    public class CancelReturnDTO
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public DateTime CancelledOn { get; set; }
        public int RefundId { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundStatus { get; set; }
        public string RefundPaymentMode { get; set; }
    }
}
