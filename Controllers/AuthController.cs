using Microsoft.AspNetCore.Mvc;
using ldap.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ldap.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost, Route("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] Login user)
        {
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }

            try
            {
                var ldapUser = _authService.Login(user.Username, user.Password);
                if (ldapUser != null)
                {
                    return Ok(ldapUser);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Local Error")
                {
                    return Unauthorized("Invalid Credentials");
                }
                return Unauthorized(ex.Message);
            }

            return Unauthorized();
        }
    }
}