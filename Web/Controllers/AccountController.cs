using Context.Data;
using Context.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class AccountController : AbstractController<Account>
    {
        public AccountController(ILogger<AccountController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }

        /// <summary>
        /// Deleting
        /// </summary>
        /// <param name="id">Account id</param>
        /// <returns></returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {            

            return StatusCode(DeleteData(await _dbContext.Account.FirstOrDefaultAsync(x => x.Id == id)));
        }
    }
}