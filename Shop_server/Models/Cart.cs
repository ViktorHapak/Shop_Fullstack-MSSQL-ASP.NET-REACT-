using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public decimal TotalPrice { get; set; } = decimal.Zero;

        public ICollection<CartItem> Items { get; set; }
            = new List<CartItem>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
