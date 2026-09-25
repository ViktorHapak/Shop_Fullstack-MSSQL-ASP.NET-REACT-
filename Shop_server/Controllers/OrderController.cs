using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shop_server.DTO;
using Shop_server.Models;
using Shop_server.Security;
using Shop_server.Services;
using Shop_server.util;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Claims;
using System.Text;

namespace Shop_server.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {

        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly UserDetailsService _userDetailsService;

        private readonly SoldDAO soldDAO;

        public OrderController(
            OrderService orderService,
            UserDetailsService userDetailsService,
            SoldDAO soldDAO,
            ProductService productService)
        {
            _orderService = orderService;
            _userDetailsService = userDetailsService;
            _productService = productService;
            this.soldDAO = soldDAO;
        }

        // GET by Id
        [HttpGet("{id:int}")]
        public ActionResult Get(int id)
        {
            try
            {
                //int Id = Convert.ToInt32(Request.Query["id"]);
                Order? order = _orderService.Find(id);
                return Ok(order);
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
        public ActionResult GetAll(int id)
        {
            try
            {
                List<Order> orders = _orderService.FindAll();
                int count = orders.Count();

                if (count == 0) return StatusCode(StatusCodes.Status204NoContent, "Üres lista");

                Dictionary<string, object> response = new()
                {
                    ["Orders"] = orders,
                    ["TotalItems"] = orders.Count
                };

                //JsonResult jsonResult = new JsonResult(response);

                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(500);
            }

        }



        // GET all
        [HttpGet("")]
        public ActionResult GetOrdersByParams(
            [FromQuery] int page = 0,
            [FromQuery] int size = 8,
            [FromQuery] int? userId = null,
            [FromQuery] string? status_name = null,
            [FromQuery] decimal? min = null,
            [FromQuery] decimal? max = null,
            [FromQuery] string? time_range = null
        )
        {

            try
            {

                List<Order> orders = _orderService.FindByParams(userId, status_name, min, max, time_range);
                PageDTO<Order> orderPageResource = convertToOrderPageResource(orders, page, size);

                return Ok(orderPageResource);
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


        //Find orders of the authorized user
        [Authorize]
        [HttpGet("own")]
        public async Task<IActionResult> GetOwnOrders([FromQuery] string? status = null)
        {
            try
            {
                UserDetails principal = await _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                );

                User user = principal.User;

                List<Order> orders = _orderService.FindOwnOrders(user, status);

                return Ok(orders);

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
        


        // Make order
        [Authorize]
        [HttpPost("order")]
        public async Task<IActionResult> MakeOrder()
        {
            try
            {
                UserDetails principal = await _userDetailsService.loadByLogin(
                    User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                );

                User user = principal.User;
                Order order = await _orderService.MakeOrder(user);

                return Ok(order);
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


        //Cancel order
        [Authorize]
        [HttpPost("cancel={id}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int id)
        {
            try
            {
                UserDetails principal = await _userDetailsService.loadByLogin(User.FindFirstValue(JwtRegisteredClaimNames.UniqueName)); 

                User user = principal.User;
                Order order = await _orderService.CancelPurchase(user, id);

                return Ok(order);
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


        //Cancel order
        [HttpPost("reject={id}")]
        public async Task<IActionResult> RejectOrder([FromRoute] int id)
        {
            try
            {
                Order order = await _orderService.RejectOrder(id);
                return Ok(order);
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


        //Apprive order
        [HttpPost("confirm={id}")]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            try
            {
                Order order = await _orderService.AppriveOrder(id);
                return Ok(order);
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

        //Compute actual day's revenue
        [HttpPut("income")]
        public async Task<IActionResult> ComputeActualIncome()
        {
            try
            {
                decimal income = _orderService.CountIncome("today");

                await soldDAO.RewriteJsonAsync(income);

                SoldData data = new SoldData
                {
                    Date = DateTime.UtcNow.Date,
                    Income = income
                };

                return Ok(data);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        //Read actual day's revenue
        [HttpGet("income")]
        public async Task<IActionResult> GetActualIncome()
        {
            try
            {
                //Read income-date from json-serialization
                Decimal income = (await soldDAO.ReadJSON()).Income;
                DateTime today = DateTime.UtcNow;
                Dictionary<string, object> response = new Dictionary<string, object>();
                response.Add("date", today);
                response.Add("income", income);

                return Ok(response);
            }
            catch (InvalidDataException)
            {
                return StatusCode(StatusCodes.Status204NoContent, "Bevétel beolvasása sikertelen");
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



        [HttpGet("incomes")]
        public async Task<ActionResult> GetMinMaxIncome([FromQuery] string? timeRange = null, [FromQuery] string? status_name = null)
        {
            try
            {
                var (Min, Max) = await _orderService.CountMaxMinIncomes(timeRange, status_name);
                Dictionary<string, object> response = new Dictionary<string, object>();
                response.Add("min", Min);
                response.Add("max", Max);

                return Ok(response);
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


        //Apprive order
        [HttpDelete("/expired")]
        public async Task<IActionResult> CleanExpired()
        {
            try
            {
                int count = await _orderService.DeleteIfExpired();
                return StatusCode(StatusCodes.Status204NoContent, "Lejárt rendelések törölve: " + count);
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


        private PageDTO<Order> convertToOrderPageResource(List<Order> orders, int page, int size)
        {
            int totalItems = orders.Count;

            List<Order> pageItems = orders
                .Skip(page * size)
                .Take(size)
                .ToList();

            return new PageDTO<Order>
            {
                Items = pageItems,
                TotalItems = totalItems,
                Size = size,
                Page = page
            };
        }
    }
}
