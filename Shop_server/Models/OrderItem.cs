using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class OrderItem
    {

        public int Id { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        public Order Order { get; set; } = null!;

        public int? ProductId { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
