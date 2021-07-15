using Microsoft.AspNetCore.Mvc;
using ldap.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ldap.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthenticationService _ldapService;
        public UserController(IAuthenticationService ldapService)
        {
            _ldapService = ldapService;
        }

        [HttpGet, Route("")]
        [AllowAnonymous]
        public IActionResult Users()
        {
            try
            {
                var ldapUsers = _ldapService.Users();
                if (ldapUsers != null)
                {
                    return Ok(ldapUsers);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }

            return Problem();
        }
    }
}