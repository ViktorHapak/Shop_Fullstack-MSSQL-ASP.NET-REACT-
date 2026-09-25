using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shop_server.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Túl rövid kategória-elnevezés!")]
        public string Name { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Product> Products { get; set; }
        = new List<Product>();
    }
}
