using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; } = null!;

        public string CustomerUsername { get; set; } = null!;

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SubmittedAt { get; set; } = null;

        public ICollection<OrderItem> Items { get; set; }
            = new List<OrderItem>();
    }
}
