using Microsoft.EntityFrameworkCore;
using Shop_server.Data;
using Shop_server.Models;
using System.Linq.Expressions;
using static NuGet.Packaging.PackagingConstants;

namespace Shop_server.Services
{
    public class OrderService
    {

        private ShopDbContext context;

        public OrderService(ShopDbContext context)
        {
            this.context = context;
        }

        public Order Find(int id)
        {
            return context.Orders
                .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefault(order => order.Id == id) ?? throw new KeyNotFoundException("A rendelés nem található");
        }

        public List<Order> FindAll()
        {
            return context.Orders
                .AsNoTracking()
                .Include(order => order.Items)
                .ToList();
        }

        public List<Order> FindByParams(int? userId, string? status_name, decimal? minPrice, decimal? maxPrice, string? time_range)
        {
            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .Include(order => order.Items);

            if (userId.HasValue)
            {
                query = query.Where(order => order.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status_name))
            {
                if (!Enum.TryParse<OrderStatus>(status_name, ignoreCase: true, out OrderStatus status))
                    throw new KeyNotFoundException("Ismeretlen rendelési állapot");

                query = query.Where(order => order.Status == status);
            }

            if (minPrice is not null)
            {
                query = query.Where(order => order.TotalPrice >= minPrice);
            }

            if (maxPrice is not null)
            {
                query = query.Where(order => order.TotalPrice <= maxPrice);
            }

            List<Order> orders = query.ToList();

            if (!string.IsNullOrWhiteSpace(time_range))
            {
                DateTime now = DateTime.UtcNow;

                orders = time_range switch
                {
                    "today" => orders
                        .Where(order =>
                            ActualDate(order).Date == now.Date)
                        .ToList(),

                    "day" => orders
                        .Where(order =>
                            ActualDate(order) > now.AddDays(-1))
                        .ToList(),

                    "last_day" => orders
                        .Where(order =>
                            ActualDate(order).Date == now.AddDays(-1).Date)
                        .ToList(),

                    "week" => orders
                        .Where(order =>
                            ActualDate(order) > now.AddDays(-7))
                        .ToList(),

                    "month" => orders
                        .Where(order =>
                            ActualDate(order) > now.AddMonths(-1))
                        .ToList(),

                    "last_month" => orders
                        .Where(order =>
                            ActualDate(order).Year == now.AddMonths(-1).Year &&
                            ActualDate(order).Month == now.AddMonths(-1).Month)
                        .ToList(),

                    _ => throw new ArgumentOutOfRangeException(
                        nameof(time_range),
                        "Ismeretlen időintervallum")
                };
            }

            orders = orders.OrderByDescending(order => order.CreatedAt).ToList();

            return orders;
        }

        public List<Order> FindOwnOrders(User user, string? status_name)
        {
            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .Where(order => order.UserId == user.Id)
                .Include(order => order.Items);

            if (!string.IsNullOrWhiteSpace(status_name))
            {
                if (!Enum.TryParse<OrderStatus>(status_name, ignoreCase: true, out OrderStatus status))
                    throw new KeyNotFoundException("Ismeretlen rendelési állapot");

                query = query.Where(order => order.Status == status);
            }

            List<Order> orders = query.OrderByDescending(order => order.CreatedAt).ToList();

            return orders;

        }

        public async Task<(decimal, decimal)> CountMaxMinIncomes(string? timeRange, string? status_name)
        {
            List<Order> actualOrders =
               FindByParams(null, status_name, null, null, timeRange);

            if (actualOrders.Count == 0)
                return (0, 0);

            decimal minPrice = actualOrders.Min(order => order.TotalPrice);
            decimal maxPrice = actualOrders.Max(order => order.TotalPrice);

            return (minPrice, maxPrice);

        }

        public decimal CountIncome(string? time_range)
        {
            DateTime now = DateTime.UtcNow;
            DateTime today = now.Date;
            DateTime tomorrow = today.AddDays(1);

            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .Where(order =>
                    order.Status == OrderStatus.Completed &&
                    order.SubmittedAt != null);

            if (!string.IsNullOrWhiteSpace(time_range))
            {
                query = time_range switch
                {
                    "today" => query.Where(order =>
                        order.SubmittedAt >= today &&
                        order.SubmittedAt < tomorrow),

                    "day" => query.Where(order =>
                        order.SubmittedAt >= today.AddDays(-1) &&
                        order.SubmittedAt < today),

                    "last_day" => query.Where(order =>
                        order.SubmittedAt >= today.AddDays(-1) &&
                        order.SubmittedAt < today),

                    "week" => query.Where(order =>
                        order.SubmittedAt >= today.AddDays(-7) &&
                        order.SubmittedAt < tomorrow),

                    "month" => query.Where(order =>
                        order.SubmittedAt >= today.AddMonths(-1) &&
                        order.SubmittedAt < tomorrow),

                    _ => throw new ArgumentOutOfRangeException(
                        nameof(time_range),
                        "Ismeretlen időintervallum"
                    )
                };
            }

            return query.Sum(order => order.TotalPrice);
        }

        public async Task<Order> MakeOrder(User user)
        {
            Cart cart = await context.Carts
                .Include(cart => cart.Items)
                .Where(cart => cart.UserId == user.Id)
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("A bevásárlókosara üres");

            Order? order = await context.Orders
                .Include(order => order.Items)
                .Where(order => order.UserId == user.Id && order.Status == OrderStatus.Pending)
                .FirstOrDefaultAsync() ?? null;

            if (order is null) await MakeUnsynchronizedError(cart);

            if (!ValidateOrderAndCart(order,cart)) await MakeUnsynchronizedError(cart);

            order.Status = OrderStatus.Waiting;
            context.Remove(cart);
            await context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> CancelPurchase(User user, int id)
        {
            IQueryable<Order> orders = context.Orders
                .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                .Where(order => order.UserId == user.Id && order.Status == OrderStatus.Waiting) 
                 ?? throw new KeyNotFoundException("Nem található leadott rendelése!");

            Order order = await orders
                .Where(order => order.Id == id)
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Nem található leadott rendelése!");

            foreach (var item in order.Items)
            {
                item.Product.Stock += item.Quantity;
            }

            context.Orders.Remove(order);
            context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> RejectOrder(int id)
        {
            Order order = Find(id);

            if (order.Status != OrderStatus.Waiting) 
                throw new InvalidOperationException("A rendelés nincs várakozó állapotban!");

            foreach (var item in order.Items)
            {
                item.Product.Stock += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;
            context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> AppriveOrder(int id)
        {
            Order order = Find(id);

            if (order.Status != OrderStatus.Waiting)
                throw new InvalidOperationException("A rendelés nincs várakozó állapotban!");

            order.Status = OrderStatus.Completed;
            order.SubmittedAt = DateTime.Now;
            context.SaveChangesAsync();
            return order;
        }

        public async Task<int> DeleteIfExpired()
        {
            int count = context.Orders
                .Where(order => ((order.Status == OrderStatus.Waiting || order.Status == OrderStatus.Cancelled)
                        && order.CreatedAt < DateTime.UtcNow.AddDays(-1)) )
                .Count();

            context.Orders
                .Where(order => ((order.Status == OrderStatus.Waiting || order.Status == OrderStatus.Cancelled)
                        && order.CreatedAt < DateTime.UtcNow.AddDays(-1)))
                .ExecuteDelete();

            await context.SaveChangesAsync();
            return count;
        }


        //Helper functions
        private async Task MakeUnsynchronizedError(Cart cart)
        {
            foreach (var item in cart.Items)
            {
                Product product = item.Product;
                product.Stock += item.Quantity;
            }

            context.Remove(cart);

            context.Orders
                .Where(order => order.UserId == cart.UserId && order.Status == OrderStatus.Pending)
                .ExecuteDelete();

            await context.SaveChangesAsync();
            throw new InvalidOperationException("Hiba keletkezett a rendelés feldolgozása során!");
        }

        private Boolean ValidateOrderAndCart(Order order, Cart cart)
        {
            if (order.Status != OrderStatus.Pending) return false;
            if (order.TotalPrice  != cart.TotalPrice) return false;
            if (order.UserId != cart.UserId) return false;
            if (order.Items.Count != cart.Items.Count) return false;

            foreach (CartItem item in cart.Items)
            {
                Boolean matches = order.Items
                    .Any(orderItem => orderItem.ProductId == item.ProductId && orderItem.Quantity == item.Quantity);

                if (!matches) return false;
            }

            return true;

        }

        private DateTime ActualDate(Order order)
        {
            return order.Status == OrderStatus.Completed && order.SubmittedAt.HasValue
                ? order.SubmittedAt.Value
                : order.CreatedAt;
        }

    }
}
