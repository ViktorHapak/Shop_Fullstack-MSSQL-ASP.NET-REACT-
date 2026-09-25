using Microsoft.EntityFrameworkCore;
using Shop_server.Data;
using Shop_server.Models;
using Shop_server.Security;
using System.Text;
using System.Text.Encodings.Web;

namespace Shop_server.Services
{
    public class UserService
    {

        private ShopDbContext context;

        public UserService(ShopDbContext context)
        {
            this.context = context;
        }

        public User Find(int id)
        {
            return context.Users.Find(id) ??
                throw new KeyNotFoundException("A felhasználó nem található!");
        }

        public List<User> FindAll()
        {
            return context.Users.ToList();
        }

        public List<User> FindByUserName(string title)
        {
            return context.Users
                .Where(user => user.Username.Contains(title)).ToList();
        }

        public List<User> FindByRole(string role_name)
        {
            if (!Enum.TryParse<Role>(role_name, ignoreCase: true, out Role role))
            {
                throw new KeyNotFoundException("Nincs ilyen jogosultság");
            }

            return context.Users
                .Where(user => user.Role == role).ToList();
        }

        public async Task<User> Create(User user)
        {
            List<string> errorMessage = new List<string>();
            bool usernameExists = await context.Users.AnyAsync(item => item.Username == user.Username);
            if (usernameExists) errorMessage.Add("A felhasználónév foglalt!");

            bool emailExists = await context.Users.AnyAsync(item => item.Email == user.Email);
            if (usernameExists) errorMessage.Add("Az email foglalt!");

            if (usernameExists || emailExists) throw new InvalidOperationException(string.Join("; ", errorMessage));


            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public async Task<User> AddModeratorRole(int  id)
        {
            User user = context.Users.Find(id) ??
                throw new KeyNotFoundException("A felhasználó nem található");

            user.Role = Role.Moderator;
            context.Carts.Where(chart =>  chart.UserId == id).ExecuteDeleteAsync();

            await context.SaveChangesAsync();
            return user;
        }

        public async Task<User> AddAdminRole(int id)
        {
            User user = context.Users.Find(id) ??
                throw new KeyNotFoundException("A felhasználó nem található");

            if (context.Users.Where(user => user.Role == Role.Admin).Count() >= 3)
                throw new InvalidOperationException("Nem lehet több adminisztrátor!");

            user.Role = Role.Admin;
            context.Carts.Where(chart => chart.UserId == id).ExecuteDeleteAsync();

            await context.SaveChangesAsync();
            return user;
        }

        public async Task<User> SetVisitorRole(int id)
        {
            User user = context.Users.Find(id) ??
                throw new KeyNotFoundException("A felhasználó nem található");

            user.Role = Role.Visitor;
            await context.SaveChangesAsync();
            return user;
        }


        public async Task<User> DeleteUser(int id)
        {
            User userToDelete = context.Users.Find(id) ??
                throw new KeyNotFoundException("A felhasználó nem található");

            context.Users.Remove(userToDelete);
            await context.SaveChangesAsync();
            return userToDelete;
        }



        public async Task<int> Count()
        {
            return await context.Users.CountAsync();
        }
    }
}
