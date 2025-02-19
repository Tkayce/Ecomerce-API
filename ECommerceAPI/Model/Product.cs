using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceAPI.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Stock { get; set; }
    }
}
