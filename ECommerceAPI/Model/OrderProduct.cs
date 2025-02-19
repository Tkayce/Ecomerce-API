using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceAPI.Model
{
    public class OrderProduct
    {


        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }

    
}
