using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly ZerdaContext _dbContext;

        public UserController(ILogger<UserController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get request
        /// </summary>
        /// <param name="limit">count records to get (max 50)</param>
        /// <param name="offset">starting position relative to the beginning of the table</param>
        /// <returns>List of objects</returns>
        /// <response code="200">Success</response>
        [ProducesResponseType(typeof(List<User>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<List<User>> Get(
            [Range(1, 50, ErrorMessage = "limit range err")] 
            [FromQuery] 
                int limit = 50, 
            [Range(0, Double.MaxValue, ErrorMessage = "offset must be non-negative")] 
            [FromQuery]
                int offset = 0)
        {
            return await _dbContext.User.Skip(offset).Take(Math.Min(50, limit)).ToListAsync();
        }  

        /// <summary>
        /// Adding
        /// </summary>
        /// <response code="200">Never return</response>\
        /// <response code="201">Success adding</response>
        /// <response code="204">Duplicate object (state unchanged)</response>
        /// <returns>Created object</returns>
        [ProducesResponseType(typeof(User), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] User obj)
        {
            try
            {
                await _dbContext.AddAsync(obj);
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, obj);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    _logger.LogWarning("Попытка добавления дубликата");
                    return StatusCode(204);
                }
                return StatusCode(500, "DbException");
            }
            catch
            {
                return StatusCode(500);
            }            
        }

        /// <summary>
        /// Deleting
        /// </summary>
        /// <param name="id">Obj id</param>
        /// <returns></returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        { 
            try
            {
                User? obj = _dbContext.User.FirstOrDefault(x => x.Id == id);
                if (obj is null)
                {
                    return NotFound();
                }
                else
                {
                    _dbContext.Entry(obj).State = EntityState.Deleted;
                    await _dbContext.SaveChangesAsync();
                    return StatusCode(204);
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException 
                    && ex.InnerException.Message.Contains("Cannot delete or update a parent row"))
                {
                    _logger.LogWarning("Попытка удаления связанной записи");
                    return StatusCode(409);
                }
                return StatusCode(500, "DbException");
            }
            catch
            {
                return StatusCode(500);
            }
            
        }
    }
}