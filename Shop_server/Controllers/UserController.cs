using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Shop_server.DTO;
using Shop_server.Models;
using Shop_server.Services;
using Shop_server.util;

namespace Shop_server.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly UserService _userService;


        public UserController(UserService userService)
        {
            _userService = userService;
        }


        [HttpGet("{id:int}")]
        public ActionResult Get([FromRoute] int id)
        {
            try
            {
                //int Id = Convert.ToInt32(Request.Query["id"]);
                User? user = _userService.Find(id);
                return Ok(user);
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
                List<User> users = _userService.FindAll();
                int count = users.Count();

                if (count == 0) return StatusCode(StatusCodes.Status204NoContent, "Üres lista");

                Dictionary<string, object> response = new()
                {
                    ["Users"] = users,
                    ["TotalItems"] = users.Count
                };

                return Ok(response);

            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }


        [HttpGet("")]
        public ActionResult GetUsersByParams(
            [FromQuery] int page = 0,
            [FromQuery] int size = 8,
            [FromQuery] string? title = null,
            [FromQuery] string? role = null)
        {
            try
            {

                List<User> users;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    users = _userService.FindByUserName(title);
                }
                else if (!string.IsNullOrWhiteSpace(role))
                {
                    users = _userService.FindByRole(role);
                }
                else
                {
                    users = _userService.FindAll();
                }


                PageDTO<User> userPageResource = convertToUserPageResource(users, page, size);

                return Ok(userPageResource);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (ArgumentOutOfRangeException e2)
            {
                return BadRequest(e2.Message);
            }
            catch (Exception e3)
            {
                return StatusCode(500);
            }
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ModifyAuthorities([FromRoute] int id, [FromQuery] string role = "Visitor")
        {
            try
            {
                User? user = _userService.Find(id);

                user = role.ToLowerInvariant() switch
                {
                    "moderator" => await _userService.AddModeratorRole(id),
                    "admin" => await _userService.AddAdminRole(id),
                    _ => await _userService.SetVisitorRole(id)
                };

                Dictionary<string, Object> response = new Dictionary<string, Object> {
                    ["user"] = user.Username,
                    ["id"] = user.Id,
                    ["role"] = role
                };

                return Ok(response);
            }
            catch (KeyNotFoundException e)
            {
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e2)
            {
                return BadRequest(e2.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {

                User? user = await _userService.DeleteUser(id);


                return StatusCode(StatusCodes.Status204NoContent, "Felhasználó eltávolítva: " + user.Username);
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


        private PageDTO<User> convertToUserPageResource(List<User> users, int page, int size)
        {
            int totalItems = users.Count;

            List<User> pageItems = users
                .Skip(page * size)
                .Take(size)
                .ToList();

            return new PageDTO<User>
            {
                Items = pageItems,
                TotalItems = totalItems,
                Size = size,
                Page = page
            };
        }
    }
}
