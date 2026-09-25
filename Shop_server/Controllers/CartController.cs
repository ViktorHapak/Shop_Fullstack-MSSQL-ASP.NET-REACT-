using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NuGet.Packaging.Licenses;
using Shop_server.DTO;
using Shop_server.Models;
using Shop_server.Security;
using Shop_server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Claims;
using System.Text;

namespace Shop_server.Controllers
{


    [Route("api/carts")]
    [ApiController]
    public class CartController : ControllerBase
    {

        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly UserDetailsService _userDetailsService;

        public CartController(
            CartService cartService, 
            UserDetailsService userDetailsService,
            ProductService productService)
        {
            _cartService = cartService;
            _userDetailsService = userDetailsService;
            _productService = productService;
        }


        [HttpGet("{id:int}")]
        public ActionResult Get(int id)
        {
            try
            {
                //int Id = Convert.ToInt32(Request.Query["id"]);
                Cart? cart = _cartService.Find(id);
                return Ok(cart);
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


        [HttpGet("all")]
        public ActionResult GetAll()
        {
            try
            {
                List<Cart> carts = _cartService.FindAll();
                int count = carts.Count();

                if (count == 0) return StatusCode(StatusCodes.Status204NoContent, "Üres lista");

                Dictionary<string, object> response = new()
                {
                    ["Carts"] = carts,
                    ["TotalItems"] = carts.Count
                };

                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("")]
        public ActionResult GetByParams([FromQuery] int? userId = null, [FromQuery] List<int>? productIds = null, [FromQuery] string operation = "and")
        {
            try
            {
                List<Cart> carts = _cartService.FindByParams(userId, productIds, operation);
                return Ok(carts);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException e2)
            {
                return BadRequest("Nem megfelelő formátum!");
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


        [HttpGet("userId={id}")]
        public ActionResult GetByUser([FromRoute] int id)
        {
            try 
            {
                Cart cart = _cartService.FindByUser(id);
                return Ok(cart);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException e2)
            {
                return BadRequest("Nem megfelelő formátum!");
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


        [HttpGet("productId={id}")]
        public async Task<IActionResult> GetByProduct([FromRoute] int id)
        {
            try
            {
                List<Cart> carts = await _cartService.FindByProductAsync(id);
                return Ok(carts);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (FormatException e2)
            {
                return BadRequest("Nem megfelelő formátum!");
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

        [Authorize]
        [HttpGet("own")]
        public ActionResult GetOwnCart()
        {
            try
            {
                UserDetails principal = _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ).Result;

                User user = principal.User;

                Cart cart = _cartService.FindOwn(user);

                if (cart is null) return StatusCode(StatusCodes.Status204NoContent, "Üres bevásárlólista!");

                return Ok(cart);
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


        [HttpGet("{id:int}")]
        public ActionResult GetItems(int id)
        {
            try
            {
                //int Id = Convert.ToInt32(Request.Query["id"]);
                Cart? cart = _cartService.Find(id);

                if (cart is null)
                {
                    return BadRequest("A bevásárlókosár nem található: " + id);
                }

                List<CartItem> items = _cartService.FindItems(id);

                return Ok(items);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

        }

        //Create new Cart
        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> CreateCart()
        {
            try
            {
                UserDetails principal = await _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                );

                User user = principal.User; 

                var (cart, order) = await _cartService.Create(user);

                return StatusCode(StatusCodes.Status201Created,cart);
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
                return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = "Belső szerverhiba.",
                    Detail = e2.Message
                }
        );
            }
        }

        //Add product to Cart
        [Authorize]
        [HttpPost("add={id}")]
        public async Task<IActionResult> AddProduct(int id)
        {
            try
            {
                UserDetails principal = await _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                );

                User user = principal.User;
                Product product = _productService.Find(id);
                Cart cart = await _cartService.AddProduct(user, product);

                return Ok(cart);
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
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "Belső szerverhiba.",
                        Detail = e2.Message,
                        Inner = e2.InnerException?.Message ?? ""
                    });
            }
        }

        //Remove product from Cart
        [Authorize]
        [HttpPost("remove={id}")]
        public async Task<IActionResult> RemoveProduct(int id)
        {
            try
            {
                UserDetails principal = _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                ).Result;

                User user = principal.User;
                Product product = _productService.Find(id);
                Cart? cart = await _cartService.RemoveProduct(user, product);

                if (cart is not null) return Ok(cart);
                else return StatusCode(StatusCodes.Status204NoContent, "Üres bevásárlólista!");


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


        //Delete Cart
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {

                Cart? cart = await _cartService.Delete(id);
                return StatusCode(StatusCodes.Status204NoContent, "Bevásárlókosár törölve: " + cart.Id);
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


        //Delete expired carts
        [HttpDelete("expired")]
        public async Task<IActionResult> DeleteExpired()
        {
            try
            {

                int count = await _cartService.DeleteIfExpired();

                return StatusCode(StatusCodes.Status204NoContent, "Lejárt bevásárlókosarak eltávolítva: " + count);
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
    }

}
