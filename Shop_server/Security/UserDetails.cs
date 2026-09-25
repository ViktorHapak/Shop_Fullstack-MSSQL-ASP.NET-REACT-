using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using Shop_server.Models;

namespace Shop_server.Security
{
    public sealed record UserDetails
    {
        public int Id { get; init; }

        public string Username { get; init; }

        public string Email { get; init; }

        public string Login { get; set; }

        public string LoginType { get; set; } = "default";

        public string PasswordHash { get; init; }


        public IReadOnlyCollection<Role> Roles { get; init; }

        public int? CartId { get; init; }

        public bool HasCart => CartId.HasValue;

        public bool IsAccountNonExpired { get; init; } = true;

        public bool IsAccountNonLocked { get; init; } = true;

        public bool IsCredentialsNonExpired { get; init; } = true;

        public bool IsEnabled { get; init; } = true;

        public User User { get; init; }

        public UserDetails(User user, LoginMode login_type)
        {
            ArgumentNullException.ThrowIfNull(user);

            Id = user.Id;
            Username = user.Username;
            Email = user.Email;
            Login = login_type == LoginMode.UserName ? Username : Email;
            LoginType = login_type == LoginMode.UserName ? "username" : "email";
            PasswordHash = user.Password;

            Roles = new[] { user.Role };

            CartId = user.Cart?.Id;

            User = user;
        }

        public bool IsInRole(Role role)
        {
            return Roles.Contains(role);
        }
    }
}
