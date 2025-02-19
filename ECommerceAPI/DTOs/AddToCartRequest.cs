namespace ECommerceAPI.DTOs
{
    public class AddToCartRequest
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
