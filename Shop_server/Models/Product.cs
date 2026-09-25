using Microsoft.AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class Product
    {
        public int Id { get; set; }


        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(typeof(decimal), "10.00", "999999999", ParseLimitsInInvariantCulture = true,
                ErrorMessage = "Az ár minimum 10 Ft'")]
        public decimal Price { get; set; }


        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 1;

        public int? DepartmentId { get; set; }

        public Department? Department { get; set; } = null!;

        [JsonIgnore]
        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();

        [JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}
