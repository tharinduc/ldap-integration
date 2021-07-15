using Microsoft.AspNetCore.Mvc;
using ldap.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace ldap.Controllers
{
    [Route("api/groups")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IAuthenticationService _ldapService;
        public GroupController(IAuthenticationService ldapService)
        {
            _ldapService = ldapService;
        }

        [HttpGet, Route("")]
        [AllowAnonymous]
        public IActionResult Groups()
        {
            try
            {
                var ldapGroups = _ldapService.Groups();
                if (ldapGroups != null)
                {
                    return Ok(ldapGroups);
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