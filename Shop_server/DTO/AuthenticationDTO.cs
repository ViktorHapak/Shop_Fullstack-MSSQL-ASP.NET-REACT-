using System.ComponentModel.DataAnnotations;

namespace Shop_server.DTO
{
    public class AuthenticationDTO
    {

        public string? Username { get; set; }

        public string? Email { get; set; }

        [Required]
        [MinLength(5)]
        public string Password { get; set; }

        public string Login => !string.IsNullOrWhiteSpace(Username) ? 
            Username : !string.IsNullOrWhiteSpace(Email) ?
             Email : "";


    }
}
