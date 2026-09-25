using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Configuration;
using Shop_server.DTO;
using Shop_server.Models;
using Shop_server.Security;
using Shop_server.Services;
using Shop_server.util;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace Shop_server.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly JwtUnit jwtUnit;
        private PasswordHasher hasher;
        private UserDetailsService _userDetailsService;
        private UserService _userService;
        private AuthoritiesDAO authoritiesDAO;


        public AuthController(
            UserDetailsService userDetailsService,
            PasswordHasher hasher,
            UserService userService,
            JwtUnit jwtUnit,
            AuthoritiesDAO authoritiesDAO)
        {
            this.jwtUnit = jwtUnit;
            this.hasher = hasher;
            _userDetailsService = userDetailsService;
            _userService = userService;
            this.authoritiesDAO = authoritiesDAO;
        }

        [HttpPost("login")]
        public async Task<IActionResult> AuthenticateUser(AuthenticationDTO authenticationDTO)
        {
            try
            {
                UserDetails userDetails;

                if (!string.IsNullOrWhiteSpace(authenticationDTO.Username))
                {
                    userDetails = await _userDetailsService.loadUserByName(authenticationDTO.Username);
                }
                else if (!string.IsNullOrWhiteSpace(authenticationDTO.Email))
                {
                    userDetails = await _userDetailsService.loadUserByEmail(authenticationDTO.Email);
                }
                else
                {
                    return BadRequest("Hiányos bejelentkezési adatok!");
                }

                if (!hasher.Verify(userDetails.User, userDetails.PasswordHash, authenticationDTO.Password))
                {
                    return Unauthorized("Hibás jelszó!");
                }

                var token = jwtUnit.GenerateJWT(userDetails);

                return Ok(token);
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



        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(UserDTO userDTO)
        {
            try
            {
                User user = convertToUser(userDTO);

                user = await _userService.Create(user);
                return StatusCode(StatusCodes.Status201Created, user);
            }
            catch (ArgumentException e)
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

        [Authorize]
        [HttpGet("")]
        public async Task<ActionResult> GetUserAsync()
        {
            try {
                string? login = User.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
                string? token = await HttpContext.GetTokenAsync("access_token");

                if (string.IsNullOrWhiteSpace(login))
                    return Unauthorized("A token nem tartalmaz bejelentkezési nevet.");

                UserDetails principal = await _userDetailsService.loadByLogin(login);
                User user = principal.User;
                string auth_type = principal.LoginType;

                Dictionary<String, Object> response = new Dictionary<string, object>
                {
                    ["Username"] = user.Username,
                    ["Email"] = user.Email,
                    ["Birth"] = user.Birth,
                    ["Password"] = user.Password,
                    ["Role"] = user.Role.ToString(),
                    ["Cart"] = user.Cart,
                    ["Authentication"] = auth_type,
                    ["Token"] = token
                };

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

        [Authorize]
        [HttpGet("authorities")]
        public async Task<ActionResult> GetAuthorities()
        {
            try
            {
                AuthoritiesData authorities = await authoritiesDAO.ReadAuthorities();
                return Ok(authorities);
            }
            catch (InvalidDataException e)
            {
                return BadRequest("Hiba történt az olvasás során!");
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }

        [Authorize]
        [HttpGet("authority={authority_name}")]
        public async Task<ActionResult> GetAuthority([FromRoute] String authority_name = "")
        {
            try
            {
                bool access = await authoritiesDAO.ReadAuthority(authority_name);

                Dictionary<String, String> response = new Dictionary<String, String>() 
                {
                    ["authority"] = authority_name,
                    ["access"] = access.ToString()
                };


                return Ok(response);
            }
            catch (InvalidDataException e)
            {
                return BadRequest("Hiba történt az olvasás során!");
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }

        [Authorize]
        [HttpPost("authorities")]
        public async Task<ActionResult> WriteAuthority(AuthoritiesData authorities)
        {
            try
            {

                //await authoritiesDAO.WriteAuthority(authority_name, rule);

                /*Dictionary<String, String> response = new Dictionary<String, String>()
                {
                    ["authority"] = authority_name,
                    ["access"] = rule.ToString()
                };*/

                await authoritiesDAO.WriteAuthorities(authorities);

                return Ok(authorities);
            }
            catch (InvalidDataException e)
            {
                return BadRequest("Hiba történt az olvasás során!");
            }
            catch (Exception e2)
            {
                return StatusCode(500);
            }

        }


        private User convertToUser(UserDTO userDTO)
        {

            User user = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                Birth = userDTO.Birth
            };

            user.Password = hasher.Hash(user, userDTO.Password);
            return user;
        }
    }
}
