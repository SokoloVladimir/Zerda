using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly ILogger<AssignmentController> _logger;

        private readonly ZerdaContext _dbContext;

        public AssignmentController(ILogger<AssignmentController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// Метод на получение назначений
        /// </summary>
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType(typeof(IEnumerable<Assignment>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            return StatusCode(200, await _dbContext.Assignment               
                .AsNoTracking()
                .Include(x => x.Work)
                .Include(x => x.Group)
                .OrderBy(x => x.WorkId).ThenBy(x => x.GroupId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }
        #endregion

        #region POST
        /// <summary>
        /// Добавление назначения
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное добавление</response>
        /// <response code="204">Попытка добавления дубликата (status quo)</response>
        /// <returns>Созданный объект</returns>
        [ProducesResponseType(typeof(Assignment), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Assignment obj)
        {
            try
            {
                obj.AssignedDate = obj.AssignedDate ?? DateTime.Now;
                _dbContext.Assignment.Entry(obj).State = EntityState.Added;
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
        #endregion

        #region PUT
        /// <summary>
        /// Обновление назначения
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное обновление</response>
        /// <returns>обновленный объект</returns>
        [ProducesResponseType(typeof(Assignment), (int)HttpStatusCode.Created)]
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] Assignment obj)
        {
            try
            {
                Assignment? Assignment = await _dbContext.Assignment
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.WorkId == obj.WorkId && d.GroupId == obj.GroupId);
                if (Assignment is not null)
                {
                    _dbContext.Entry(obj).State = EntityState.Modified;
                }
                else
                {
                    _dbContext.Entry(obj).State = EntityState.Added;
                }

                obj.AssignedDate = obj.AssignedDate ?? DateTime.Now;
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Удаление назначения
        /// </summary>
        /// <param name="workId">идентификатор работы</param>
        /// <param name="groupId">идентификатор группы</param>
        /// <returns>HTTP ответ</returns>
        /// <response code="200">Успешное удаление</response>
        /// <response code="404">Объект для удаления не найден (status quo)</response>
        /// <response code="409">Существует некаскадная связь (status quo)</response>
        [HttpDelete("{workId}/{groupId}")]
        public async Task<IActionResult> Delete(int workId, int groupId)
        {
            try
            {
                Assignment? obj = _dbContext.Assignment.FirstOrDefault(x => x.WorkId == workId && x.GroupId == groupId);
                if (obj is null)
                {
                    return NotFound();
                }
                else
                {
                    _dbContext.Entry(obj).State = EntityState.Deleted;
                    await _dbContext.SaveChangesAsync();
                    return StatusCode(200);
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
        #endregion
    }
}