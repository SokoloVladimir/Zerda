using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Web.Controllers.V2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    [ApiVersion("2.0")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        private readonly ZerdaContext _dbContext;

        public AccountController(ILogger<AccountController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Метод для тестирования версионирования API
        /// </summary>
        /// <response code="200">Успех</response>
        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            return Ok();
        }       
    }
}