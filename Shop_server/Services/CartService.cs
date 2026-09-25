using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Shop_server.Data;
using Shop_server.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Shop_server.Services
{
    public class CartService
    {

        private ShopDbContext context;

        public CartService(ShopDbContext context)
        {
            this.context = context;
        }

        public Cart Find(int id)
        {
            return context.Carts.Find(id) ?? throw new KeyNotFoundException("A bevásárlókosár nem található");
        }

        public List<Cart> FindAll()
        {
            return context.Carts
                .AsNoTracking()
                .Include(cart => cart.Items)
                .ToList();
        }

        //Loading CartItems as well - eager loading
        public Cart? FindByUser(int userId)
        {
            return context.Carts
                .AsNoTracking()
                .Include(cart => cart.Items)
                .FirstOrDefault(chart => chart.UserId == userId) ?? null;
        }

        public Cart? FindOwn(User user)
        {
            return context.Carts
                .Include(cart => cart.Items)
                .FirstOrDefault(chart => chart.UserId == user.Id) ?? null;
        }

        public List<CartItem> FindItems(int id)
        {
            return context.CartItems.Where(chart => chart.Id == id).ToList();
        }

        public async Task<List<Cart>> FindByProductAsync(int productId)
        {
            return await context.Carts
                .AsNoTracking()
                .Include(cart => cart.Items)
                .Where(cart =>
                    cart.Items.Any(item =>
                        item.ProductId == productId))
                .ToListAsync();
        }

        public List<Cart> FindByParams(int? userId, List<int>? productIds, string operation = "and")
        {
            List<Cart> carts;

            IQueryable<Cart> query = context.Carts
                .AsNoTracking()
                .Include(cart => cart.User)
                .Include(cart => cart.Items)
                .ThenInclude(item => item.Product);

            if (userId.HasValue)
            {
                query = query.Where(chart => chart.UserId == userId);
            }

            if (productIds is not null)
            {
                switch (operation?.ToLower())
                {
                    case "and":
                        {
                            foreach (var productId in productIds)
                            {
                                query = query.Where(cart => cart.Items.Any(item => item.ProductId == productId));
                            }
                            break;
                        }
                    case "or":
                        {
                          
                            query = query.Where(cart => cart.Items.Any(item => productIds.Contains(item.ProductId)));
                            
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(operation),
                            "Ismeretlen logikai művelet!"
                        );
                }
            }

            carts = query.ToList();
            return carts;
        }

        public async Task<(Cart,Order)> Create(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            bool cartExists = await context.Carts.AnyAsync(cart => cart.UserId == user.Id);

            if (cartExists) throw new InvalidOperationException("Csak egy aktív bevásárlókosara lehet");

            Cart cart = new () 
            { 
                UserId = user.Id,
            };

            Order order = new()
            {
                UserId = user.Id,
                CustomerUsername = user.Username
            };

            context.Carts.Add(cart);
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return (cart,order);
        }

        public async Task<Cart> AddProduct(User user, Product product)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(product);

            Cart cart;
            Order order;

            if (user.Cart is null)
            {
                (cart,order) = await Create(user);

                if (product.Stock > 0) product.Stock--;
                else throw new InvalidOperationException("A termék nem található a készleten!");

                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    UnitPrice = product.Price
                });

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    UnitPrice = product.Price
                });

               

            }
            else 
            { 
                cart = FindOwn(user) ?? throw new KeyNotFoundException("Nem rendelkezik bevásárlókosárral!");

                order = context.Orders
                .Include(order => order.Items)
                .Where(order => order.UserId == user.Id && order.Status == OrderStatus.Pending)
                .FirstOrDefault() ?? throw new KeyNotFoundException("A kosár nincs szinkronizálva!");

                if (cart.Items.Any(item => item.ProductId == product.Id))
                {
                    CartItem cartItem = cart.Items.FirstOrDefault(item => item.ProductId == product.Id) ??
                        throw new KeyNotFoundException("A termék nem található a kosárban!"); ;

                    OrderItem orderItem = order.Items.FirstOrDefault(item => item.ProductId == product.Id) ??
                        throw new KeyNotFoundException("A termék nem található a rendelésben."); ;

                    if (product.Stock > 0) product.Stock--;
                    else throw new InvalidOperationException("A termék nem található a készleten!");

                    cartItem.Quantity++;
                    orderItem.Quantity++;
                }
                else
                {
                    if (product.Stock > 0) product.Stock--;
                    else throw new InvalidOperationException("A termék nem található a készleten!");

                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.Id,
                        UnitPrice = product.Price
                    });

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = product.Price
                    });                 
                }  
            }

            cart.TotalPrice = cart.Items.Sum(item => item.TotalPrice);
            order.TotalPrice = order.Items.Sum(item => item.TotalPrice);

            await context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> RemoveProduct(User user, Product product)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(product);

            Cart cart = FindOwn(user) ?? throw new KeyNotFoundException("Nincs hozzáadva a bevásárlólistához");
            Order order = context.Orders
                .Include(order => order.Items)
                .Where(order => order.UserId == user.Id && order.Status == OrderStatus.Pending)
                .FirstOrDefault()
                ?? throw new KeyNotFoundException("A kosár nincs szinkronizálva!"); ;

            CartItem cartItem = cart.Items.FirstOrDefault(item => item.ProductId == product.Id) ??
                throw new KeyNotFoundException("Nincs hozzáadva a bevásárlólistához");

            OrderItem orderItem = order.Items.FirstOrDefault(item => item.ProductId == product.Id) ??
                        throw new KeyNotFoundException("A termék nem található a rendelésben."); 

            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                orderItem.Quantity--;
                product.Stock++;
            }
            else
            {
                cart.Items.Remove(cartItem);
                order.Items.Remove(orderItem);
                context.Remove(cartItem);
                context.Remove(orderItem);

                product.Stock++;

                cart.TotalPrice = cart.Items.Sum(item => item.TotalPrice);
                order.TotalPrice = order.Items.Sum(item => item.TotalPrice);
            }

            if (cart.Items.IsNullOrEmpty()) {
                context.Remove(cart);
                context.Remove(order);
            }

            await context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> Delete(int id)
        {
            Cart? cartToDelete = context.Carts.Find(id) ??
                throw new KeyNotFoundException("Nincs ilyen bevásárlókosár!");

            context.Carts.Remove(cartToDelete);

            context.Orders
                .Where(order => order.UserId == cartToDelete.UserId && order.Status == OrderStatus.Pending)
                .ExecuteDelete();

            await context.SaveChangesAsync();
            return cartToDelete;
        }

        public async Task<int> DeleteIfExpired()
        {
            int count = context.Carts
                .Where(cart => cart.CreatedAt < DateTime.UtcNow.AddDays(-1))
                .Count();

            context.Carts
                .Where(cart => cart.CreatedAt < DateTime.UtcNow.AddDays(-1))
                .ExecuteDeleteAsync();

            context.Orders
                .Where(order => order.Status == OrderStatus.Pending && order.CreatedAt < DateTime.UtcNow.AddDays(-1))
                .ExecuteDeleteAsync();

            await context.SaveChangesAsync();
            return count;
        }

    }
}
