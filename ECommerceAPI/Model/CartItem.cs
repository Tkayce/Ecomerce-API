namespace ECommerceAPI.Model
{
    public class CartItem
    {

        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Store price at time of addition

        // Navigation properties
        public Cart Cart { get; set; }
        public Product Product { get; set; }
    }
}
