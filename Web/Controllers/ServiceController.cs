using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    public class ServiceControler : ControllerBase
    {
        private readonly ILogger<StudentController> _logger;

        private readonly ZerdaContext _dbContext;

        public ServiceControler(ILogger<StudentController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// Проверка состояния сервиса для server-mesh/docker
        /// </summary>
        /// <response code="200">Успех</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpGet("health")]
        public async Task<IActionResult> Get()
        {
            if (await _dbContext.Database.CanConnectAsync())
            {
                return StatusCode(200);
            }
            return StatusCode(503);
        }
        #endregion
     }
}