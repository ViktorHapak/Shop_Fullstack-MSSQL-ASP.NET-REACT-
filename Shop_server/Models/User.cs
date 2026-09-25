using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

namespace Shop_server.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A felhasználónév megadása kötelező!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Az email megadása kötelező!")]
        [EmailAddress(ErrorMessage = "Érvényes email címet adjon meg!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A születési dátum megadása kötelező!")]
        public DateTime Birth { get; set; }

        [Required(ErrorMessage = "A jelszó megadása kötelező!")]
        public String Password { get; set; }


        public Role Role { get; set; } = Role.Visitor;

        [JsonIgnore]
        public Cart? Cart { get; set; }

        [JsonIgnore]
        public ICollection<Order> Orders { get; set; }
        = new List<Order>();
    }
}
