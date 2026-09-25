using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Expressions;
using Shop_server.Data;
using Shop_server.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_server.Services
{
    public class ProductService
    {

        private ShopDbContext context;

        public ProductService(ShopDbContext context)
        {
            this.context = context;
        }

        public Product Find(int id)
        {
            return context.Products.Find(id) ?? throw new KeyNotFoundException("A termék nem található"); ;
        }

        public List<Product> FindAll()
        {
            return context.Products.ToList();
        }

        public async Task<int> Count()
        {
            return await context.Products.CountAsync();
        }

        public async Task<int> CountStock()
        {
            return await context.Products
                .SumAsync(product => product.Stock);
        }

        public decimal FindLowestPrice(List<Product> products)
        {
            if (products.Count == 0)
                return context.Products.Min(product => product.Price);
            return products.Min(product => product.Price);

            //return products.Select(product => product.Price).Min();
        }

        public decimal FindHighestPrice(List<Product> products)
        {
            if (products.Count == 0)
                return context.Products.Max(product => product.Price);
            return products.Max(product => product.Price);
        }

        public List<Product> FindByName(string name)
        {
            return context.Products
                .Where(product => product.Name.Contains(name)).ToList();
        }

        public List<Product> FindByDepartment(int departmentId)
        {
            return context.Products
                .Where(product => product.DepartmentId == departmentId).ToList();
        }

        /*public async Task<List<User>> FindAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }*/

        public List<Product> FindByParams(string title, string? deparment_name, decimal? min_price, decimal? max_price)
        {
            List<Product> products;

            IQueryable<Product> query = context.Products
                .AsNoTracking()
                .Include(product => product.Department); 

            if (!string.IsNullOrWhiteSpace(title))
            {
                string normalizedTitle = title.Trim();
                query = query.Where(product => product.Name.Contains(normalizedTitle));
            }


            if (!string.IsNullOrWhiteSpace(deparment_name))
            {
                deparment_name = deparment_name.Trim();

                if (deparment_name.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(product => product.DepartmentId == null);
                }
                else
                {
                    Department department = FindDeparmentByName(deparment_name);
                    query = query.Where(product => product.DepartmentId == department.Id);
                }
            }

            if(min_price.HasValue)
            {
                query = query.Where(product => product.Price >= min_price);
            }

            if (max_price.HasValue)
            {
                query = query.Where(product => product.Price <= max_price);
            }

            products = query.ToList();
            return products;
        }

        public async Task<Product> Create(Product product)
        {

            if (product.DepartmentId is not null)
            {
                bool departmentExists = await context.Departments
                .AnyAsync(department =>
                    department.Id == product.DepartmentId);

                if (!departmentExists)
                {
                    throw new KeyNotFoundException(
                        "Nincs ilyen részleg!");
                }
            }

            string normalizedName = product.Name.ToLowerInvariant();

            bool nameExists = await context.Products.AnyAsync(item => item.Name.Trim().ToLower() == normalizedName);
            if (nameExists) throw new InvalidOperationException("A terméknév foglalt!");

            context.Products.Add(product);
            await context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> Update(int id, Product product)
        {
            Product? productToUpdate = context.Products.Find(id);

            if (product.DepartmentId is not null)
            {
                bool departmentExists = await context.Departments
                .AnyAsync(department =>
                    department.Id == product.DepartmentId);

                if (!departmentExists)
                {
                    throw new KeyNotFoundException(
                        "Nincs ilyen részleg!");
                }
            }

            if (productToUpdate == null) throw new KeyNotFoundException("A termék nem található!");

            string normalizedName = product.Name.ToLowerInvariant();

            bool nameExists = await context.Products.AnyAsync(item => item.Name.Trim().ToLower() == normalizedName);
            if (nameExists && product.Name != productToUpdate.Name) throw new InvalidOperationException("A terméknév foglalt!");

            productToUpdate.Name = product.Name.Trim();
            productToUpdate.Price = product.Price;
            productToUpdate.Stock = product.Stock;
            productToUpdate.DepartmentId = product.DepartmentId;

            await context.SaveChangesAsync();
            return productToUpdate;
        }

        public async Task<Product> AddProduct(int id, int quantity)
        {
            Product? productToAdd = context.Products.Find(id) ??
                throw new KeyNotFoundException("A termék nem található");

            if (productToAdd == null) throw new KeyNotFoundException("A termék nem található!");
            if (quantity <= 0) throw new KeyNotFoundException("Az értéknek pozitívnak kell lennie!");

            productToAdd.Stock += quantity;
            await context.SaveChangesAsync();
            return productToAdd;
        }

        public async Task<Product> ReduceProduct(int id, int quantity)
        {
            Product? productToReduce = context.Products.Find(id) ??
                throw new KeyNotFoundException("A termék nem található");

            if (productToReduce == null) throw new KeyNotFoundException("A termék nem található!");
            if (quantity <= 0) throw new KeyNotFoundException("Az értéknek pozitívnak kell lennie!");
            if (productToReduce.Stock <= quantity) throw new InvalidOperationException("A készlet nem elegendő!");


            productToReduce.Stock -= quantity;
            await context.SaveChangesAsync();
            return productToReduce;
        }

        public async Task<Product> Delete(int id)
        {
            Product? productToDelete = context.Products.Find(id) ??
                throw new KeyNotFoundException("A termék nem található!"); ;


            context.Products.Remove(productToDelete);
            await context.SaveChangesAsync();
            return productToDelete;
        }

        public async Task<int> DeleteAllProductsAsync()
        {
            List<Product> products =
                await context.Products.ToListAsync();

            if (products.Count == 0)
            {
                return 0;
            }

            context.Products.RemoveRange(products);
            await context.SaveChangesAsync();

            return products.Count;
        }

        public Department FindDeparment(int id)
        {
            return context.Departments.Find(id) ??
                throw new KeyNotFoundException("A részleg nem található"); ;
        }

        public List<Department> FindDepartments()
        {
            return context.Departments.ToList();
        }

        public Department? FindDeparmentByName(string name)
        {
            string normalizedName = name.ToLowerInvariant().Trim();

            return context.Departments
                .Where(department => department.Name.Trim().ToLower() == normalizedName)
                .FirstOrDefault() ??
                  throw new KeyNotFoundException("A részleg nem található");
        }

        public async Task<Department> CreateDepartment(Department department)
        {
            string normalizedName = department.Name.ToLowerInvariant().Trim();

            bool nameExists = await context.Departments.AnyAsync(item => item.Name.Trim().ToLower() == normalizedName);
            if (nameExists) throw new InvalidOperationException("A területnév foglalt!");

            context.Departments.Add(department);
            await context.SaveChangesAsync();
            return department;
        }

        public async Task<Department> DeleteDepartment(int id)
        {
            Department? departmentToDelete = context.Departments.Find(id);

            if (departmentToDelete == null) throw new KeyNotFoundException("A részleg nem található!");

            context.Departments.Remove(departmentToDelete);
            await context.SaveChangesAsync();
            return departmentToDelete;
        }

        public async Task<int> DeleteAllDepartmentsAsync()
        {
            List<Department> departments =
                await context.Departments.ToListAsync();

            if (departments.Count == 0)
            {
                return 0;
            }

            context.Departments.RemoveRange(departments);
            await context.SaveChangesAsync();

            return departments.Count;
        }

        public async Task<Department> SetDepartment(int productId, int departmentId)
        {
            Department department = FindDeparment(departmentId)
                ?? throw new KeyNotFoundException(
                "A részleg nem található!"); 

            Product product = Find(productId)
                ?? throw new KeyNotFoundException(
                "A termék nem található!");


            product.DepartmentId = departmentId;
            product.Department = department;

            await context.SaveChangesAsync();
            return department;

        }

        public async Task<Department> TakeFromDepartment(int productId)
        {

            Product product = Find(productId)
                ?? throw new KeyNotFoundException(
                "A termék nem található!");


            bool departmentExists = await context.Departments
                .AnyAsync(department => department.Id == product.DepartmentId);

            if (!departmentExists)
                throw new KeyNotFoundException("A termék nem tartozik egyik részleghez sem!");

            Department department = product.Department;
               
            product.DepartmentId = null;
            product.Department = null;

            await context.SaveChangesAsync();
            return department;

        }


        public async Task<Department> CleanDepartment(int departmentId)
        {
            Department department = FindDeparment(departmentId)
                ?? throw new KeyNotFoundException("A részleg nem található!");

            /*context.Products
                .Where(product => product.DepartmentId == departmentId)
                .ForEachAsync(product => product.Stock = 0);*/

            await context.Products
                        .Where(product => product.DepartmentId == departmentId)
                        .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.Stock,0));

            await context.SaveChangesAsync();
            return department;
        }

        public async Task<Department> CleanFinallyDepartment(int departmentId)
        {
            Department department = FindDeparment(departmentId)
                ?? throw new KeyNotFoundException(
                "A részleg nem található!");

            await context.Products
                .Where(product => product.DepartmentId == departmentId)
                .ExecuteDeleteAsync();

            await context.SaveChangesAsync();
            return department;
        }
    }
}
