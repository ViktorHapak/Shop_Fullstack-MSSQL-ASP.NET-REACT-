using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Shop_server.Data;
using Shop_server.Models;

namespace Shop_server.Security
{
    public class UserDetailsService
    {

        private readonly ShopDbContext context;

        public UserDetailsService(ShopDbContext context)
        {
            this.context = context;
        }

        public async Task<UserDetails> loadUserByName(string username)
        {
            User user = await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(user => user.Username == username) ??   
                throw new KeyNotFoundException($"Nincs ilyen felhasználónév: {username} !");

            return new UserDetails(user, LoginMode.UserName);

        }

        public async Task<UserDetails> loadUserByEmail(string email)
        {
            User user = await context.Users.AsNoTracking()
                .FirstOrDefaultAsync(user => user.Email == email) ??
                throw new KeyNotFoundException($"Nincs ilyen email: {email} !");

            return new UserDetails(user, LoginMode.Email);
        }

        public async Task<UserDetails> loadByLogin(string login)
        {
            User user; LoginMode login_type;
            if (login.Contains("@"))
            {
                user = await context.Users.AsNoTracking()
                .Include(user => user.Cart)
                .FirstOrDefaultAsync(user => user.Email == login) ??
                throw new KeyNotFoundException($"Nincs ilyen email: {login}");
                login_type = LoginMode.Email;
            }
            else
            {
                user = await context.Users.AsNoTracking()
                .Include(user => user.Cart)
                .FirstOrDefaultAsync(user => user.Username == login) ??
                throw new KeyNotFoundException($"Nincs ilyen email: {login}");
                login_type = LoginMode.UserName;
            }

            return new UserDetails(user, login_type);
        }

    }
}
