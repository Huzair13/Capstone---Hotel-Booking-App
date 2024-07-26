namespace CancellationService.Models
{
    public class Refund
    {
        public int Id { get; set; }
        public decimal RefundAmount { get; set; }
        public string RefundStatus { get; set; } = "Processing";
        public string RefundPaymentMode { get; set; }
    }
}
