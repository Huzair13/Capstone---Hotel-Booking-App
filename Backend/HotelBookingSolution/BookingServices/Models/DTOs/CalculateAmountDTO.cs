namespace BookingServices.Models.DTOs
{
    public class CalculateAmountDTO
    {
        public decimal TotalAmount {  get; set; }
        public decimal FinalAmount { get; set; }
        public decimal Discount { get; set; }
    }
}
