namespace ECommerceAPI.Model
{
    public class Cart
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } // Identifies the owner of the cart
        public bool IsCheckedOut { get; set; } = false;

        // Navigation property
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
