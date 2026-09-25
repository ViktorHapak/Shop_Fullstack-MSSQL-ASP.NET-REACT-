using Azure;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using NuGet.Protocol;
using Shop_server.DTO;
using Shop_server.Models;
using Shop_server.Services;
using Shop_server.util;
using System.Net.NetworkInformation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shop_server.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly ProductService _productService;
      

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // GET by Id
        [HttpGet("{id:int}")]
        public ActionResult Get(int id)
        {
            try
            {
                //int Id = Convert.ToInt32(Request.Query["id"]);
                Product? product = _productService.Find(id);
                return Ok(product);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

        }


        // GET all
        [HttpGet("all")]
        public ActionResult GetAll()
        {
            try
            {
                List<Product> products = _productService.FindAll();
                int count = products.Count();

                if (count == 0) return StatusCode(StatusCodes.Status204NoContent,"Üres lista");

                Dictionary<string, object> response = new()
                {
                    ["Products"] = products,
                    ["TotalItems"] = products.Count
                };

                //JsonResult jsonResult = new JsonResult(response);

                return Ok(response);

            } 
            catch (Exception e)
            {
                return StatusCode(500);
            }

        }


        //Get by Parameters
        [HttpGet("")]
        public ActionResult GetProductsByParam(
            [FromQuery] int page = 0,
            [FromQuery] int size = 8,
            [FromQuery] string title = "",
            [FromQuery] string? department_name = null,
            [FromQuery] int? min_price = null,
            [FromQuery] int? max_price = null,
            [FromQuery] string? order = "default"
        )
        {
            try
            {
                List<Product> products = _productService.FindByParams(title, department_name, min_price, max_price);

                switch (order)
                {
                    case "name_inc":
                        products = products
                            .OrderBy(product => product.Name)
                            .ToList();
                        break;

                    case "name_dec":
                        products = products
                            .OrderByDescending(product => product.Name)
                            .ToList();
                        break;

                    case "price_inc":
                        products = products
                            .OrderBy(product => product.Price)
                            .ToList();
                        break;

                    case "price_dec":
                        products = products
                            .OrderByDescending(product => product.Price)
                            .ToList();
                        break;

                    case "default":
                    default:
                        break;
                }

                PageDTO<Product> productPageResource = convertToProductPageResource(products, page, size);

                decimal lowest_price = _productService.FindLowestPrice(products);
                decimal highest_price = _productService.FindHighestPrice(products);

                return Ok(new
                {
                    items = productPageResource.Items,
                    totalItems = productPageResource.TotalItems,
                    page = productPageResource.Page,
                    size = productPageResource.Size,
                    totalPages = productPageResource.TotalPages,
                    min = lowest_price,
                    max = highest_price
                });

            }
            catch (KeyNotFoundException e) 
            { 
                return BadRequest(e.Message);
            } 
            catch (ArgumentOutOfRangeException e3)
            {
                return BadRequest(e3.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }

        }


        //Count quantity of types and stock
        [HttpGet("count")]
        public async Task<IActionResult> Count()
        {
            try
            {
                int type_quantity = await _productService.Count();
                int stock = await _productService.CountStock();

                Dictionary<string, int> response = new()
                {
                    ["typeQuantity"] = type_quantity,
                    ["stock"] = stock
                };

                return Ok(response);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }


        //Create new entity
        [HttpPost("")]
        public async Task<ActionResult> CreateProduct(ProductDTO productResource)
        {
            try
            {

                Product? product = convertToProduct(productResource);
                product = await _productService.Create(product);

                return CreatedAtAction(nameof(Get), new { id = product.Id },product);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1) 
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            } 
        }


        //Update entity
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, ProductDTO productResource)
        {
            try
            {
                Product product = convertToProduct(productResource);

                product = await _productService.Update(id, product);
                return Ok(product);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }
        }


        //Update stock
        [HttpPut("stock/{id:int}")]
        public ActionResult RefreshStock([FromRoute] int id, [FromQuery] string operation, [FromQuery] int quantity = 1)
        {
            try
            {
                Product? product;

                switch (operation)
                {
                    case "inc": product = _productService.AddProduct(id, quantity).Result; break;
                    case "red": product = _productService.ReduceProduct(id, quantity).Result; break;
                    default: return BadRequest("Ismeretlen művelet");
                }

                Dictionary<string, Object> response = new Dictionary<string, Object>();
                response.Add("product", product.Name);
                response.Add("operation", operation + ": " + quantity);
                response.Add("quantity", product.Stock);

                return Ok(response);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }
        }


        //Delete product
        [HttpDelete("{id:int}")]
        public ActionResult DeleteProduct([FromRoute] int id)
        {
            try
            {

                Product? product = _productService.Delete(id).Result;

                return StatusCode(StatusCodes.Status204NoContent, "Termék törölve: " + product.Name);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        //Delete all products
        [HttpDelete("all")]
        public ActionResult DeleteAll(int id)
        {
            try
            {

                int? count = _productService.DeleteAllProductsAsync().Result;

                return StatusCode(StatusCodes.Status204NoContent, "Összes termék törölve: " + count);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        //Find department
        [HttpPost("dep/{id:int}")]
        public ActionResult GetDepartment(int id)
        {
            try
            {

                Department department = _productService.FindDeparment(id);



                return Ok(department);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        //Find departments
        [HttpGet("dep/all")]
        public ActionResult GetDepartments()
        {
            try
            {

                List<Department> departments = _productService.FindDepartments();

                Dictionary<string, object> response = new()
                {
                    ["Products"] = departments,
                    ["TotalItems"] = departments.Count
                };
                return Ok(departments);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        //Find department by Name
        [HttpGet("dep")]
        public ActionResult GetDepartmentByName([FromQuery] String name)
        {
            try
            {
                Department department = _productService.FindDeparmentByName(name);
                return Ok(department);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        //Create department
        [HttpPost("dep")]
        public async Task<ActionResult> CreateDepartment(Department departmentResource)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest("Helytelen formátum!");

                Department department = await _productService.CreateDepartment(departmentResource);

 

                return CreatedAtAction(nameof(Get), new { id = department.Id }, department);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }

        //Refresh (add/remove) product to department
        [HttpPut("dep")]
        public async Task<ActionResult> RefreshDepartment(
            [FromQuery] string operation,
            [FromQuery] int productId,
            [FromQuery] int departmentId
        )
        {
            try
            {
                Department? department;

                switch (operation)
                {
                    case "set": department = await _productService.SetDepartment(productId, departmentId); break;
                    case "reset": department = await _productService.TakeFromDepartment(productId); break;
                    default: return BadRequest("Ismeretlen művelet");
                }

                Product product = _productService.Find(productId);

                Dictionary<string,object> response = new Dictionary<string,object>();
                response.Add("deparment",department.Name);
                response.Add("product",product.Name);

                return Ok(response);
            }
            catch (KeyNotFoundException e) {
                return BadRequest(e.Message);
            } 
            catch (FormatException)
            {
                return BadRequest("Nem megfelelő formátum!");
            } 
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpPut("dep/clean")]
        public async Task<ActionResult> CleanDepartment([FromQuery] string mode = "", 
                                            [FromQuery] int departmentId = 0)
        {
            try
            {
                Department? department;

                switch (mode)
                {
                    case "stock":
                        {
                            department = await _productService.CleanDepartment(departmentId);
                            return Ok($"A részleg kisöpörve!\n{department.Name}");
                        }

                    case "type":
                        {
                            department = await _productService.CleanFinallyDepartment(departmentId);
                            return Ok($"A részleg termékei eltávolítva!\n{department.Name}");
                        }
                    default: return BadRequest("Ismeretlen művelet");
                }
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException)
            {
                return BadRequest("Nem megfelelő formátum!");
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpDelete("dep/{id:int}")]
        public async Task<ActionResult> DeleteDepartment(int id)
        {
            try
            {

                Department? department = await _productService.DeleteDepartment(id);

                return StatusCode(StatusCodes.Status204NoContent, "Részleg törölve: " + department.Name);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }

        [HttpDelete("dep/all")]
        public async Task<ActionResult> DeleteDepartments() 
        {
            try
            {

                int? count = await _productService.DeleteAllDepartmentsAsync();

                return StatusCode(StatusCodes.Status204NoContent, "Összes részleg törölve: " + count);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e1)
            {
                return BadRequest(e1.Message);
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }
        }


        private PageDTO<Product> convertToProductPageResource(List<Product> products, int page, int size)
        {
            int totalItems = products.Count;

            List<Product> pageItems = products
                .Skip(page * size)
                .Take(size)
                .ToList();

            return new PageDTO<Product>
            {
                Items = pageItems,
                TotalItems = totalItems,
                Size = size,
                Page = page
            };
        }

        private Product convertToProduct (ProductDTO productResource)
        {
            return new Product
            {
                Name = productResource.Name,
                Price = productResource.Price,
                Stock = productResource.Stock,
                DepartmentId = productResource.DepartmentId
            };

        }
    }
}
