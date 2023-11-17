using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisciplineController : ControllerBase
    {
        private readonly ILogger<DisciplineController> _logger;

        private readonly ZerdaContext _dbContext;

        public DisciplineController(ILogger<DisciplineController> logger, ZerdaContext dbContext)
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
        [ProducesResponseType(typeof(IEnumerable<Discipline>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            return StatusCode(200, await _dbContext.Discipline
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }

        /// <summary>
        /// Adding object
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="201">Success adding</response>
        /// <response code="204">Duplicate object (state unchanged)</response>
        /// <returns>Created object</returns>
        [ProducesResponseType(typeof(Discipline), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Discipline obj)
        {
            try
            {
                await _dbContext.Discipline.AddAsync(obj);
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, obj);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    _logger.LogWarning("������� ���������� ���������");
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
        /// Updating object
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="201">Success adding</response>
        /// <response code="204">Duplicate object (state unchanged)</response>
        /// <returns>Updated object</returns>
        [ProducesResponseType(typeof(Discipline), (int)HttpStatusCode.Created)]
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] Discipline obj)
        {
            try
            {                
                _dbContext.Entry(obj).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, obj);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    _logger.LogWarning("������� ���������� ���������");
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
        /// Deleting object
        /// </summary>
        /// <param name="id">required id</param>
        /// <returns>response</returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Discipline? obj = _dbContext.Discipline.FirstOrDefault(x => x.Id == id);
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
                    _logger.LogWarning("������� �������� ��������� ������");
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