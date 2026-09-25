using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        [JsonIgnore]
        public Cart Cart { get; set; } = null!;

        public int ProductId { get; set; }

       
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;

    }
}
