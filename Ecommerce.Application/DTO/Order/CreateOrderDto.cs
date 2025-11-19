namespace Ecommerce.Application.DTO.Order
{
    public class CreateOrderDto
    {
        // minimal simulated payment & optional notes
        public string? PaymentMethod { get; set; }
        public string? Notes { get; set; }
    }
}
