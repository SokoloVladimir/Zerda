using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "teacher")]
    public class WorkController : ControllerBase
    {
        private readonly ILogger<WorkController> _logger;

        private readonly ZerdaContext _dbContext;

        public WorkController(ILogger<WorkController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// Метод на получение работ
        /// </summary>
        /// <param name="disciplineId">фильтрация по дисциплине</param>
        /// <param name="workTypeId">фильтрация по типу работы</param>
        /// <param name="semesterId">фильтрация по семестру</param>
        /// <param name="groupId">фильтрация по группе (назначенные работы)</param>        
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType(typeof(IEnumerable<Work>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        [AllowAnonymous]
        public async Task<IActionResult> Get(
            int? disciplineId = null,
            int? workTypeId = null,
            int? semesterId = null,
            int? groupId = null,
            int limit = 50,
            int offset = 0
            )
        {
            if (HttpContext.User.Claims.IsNullOrEmpty())
            {
                return Unauthorized();
            }
            if (HttpContext.User.IsInRole("student"))
            {
                Account account = _dbContext.Account
                    .AsNoTracking()
                    .Include(x => x.Student)
                    .First(x => x.Login == HttpContext.User.Identity!.Name);
                if (groupId is not null && groupId != account.Student?.Id)
                {
                    return Forbid();
                }
                groupId = account.Student?.Id;
            }
            else if (!HttpContext.User.IsInRole("teacher"))
            {
                return Forbid();
            }

            List<Work> works = await _dbContext.Work
               .AsNoTracking()
               .Include(x => x.Discipline)
               .Include(x => x.WorkType)
               .Include(x => x.Semester)
               .Where(x => disciplineId == null || x.DisciplineId == disciplineId)
               .Where(x => workTypeId == null || x.WorkTypeId == workTypeId)
               .Where(x => semesterId == null || x.SemesterId == semesterId)
               .OrderBy(x => x.Id)
               .Skip(offset)
               .Take(Math.Min(limit, 50))
               .ToListAsync();

            if (groupId is not null)
            {
                List<int> cachedAssigned = await _dbContext.Assignment.Where(x => x.GroupId == groupId).Select(x => x.WorkId).ToListAsync();
                works = works.Where(x => cachedAssigned.Contains(x.Id)).ToList();
            }
            return StatusCode(200, works);
        }
        #endregion

        #region POST
        /// <summary>
        /// Добавление работы
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное добавление</response>
        /// <response code="204">Попытка добавления дубликата (status quo)</response>
        /// <returns>Созданный объект</returns>
        [ProducesResponseType(typeof(Work), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Work obj)
        {
            try
            {
                _dbContext.Work.Entry(obj).State = EntityState.Added;
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
        /// Обновление работы
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное обновление</response>
        /// <returns>обновленный объект</returns>
        [ProducesResponseType(typeof(Work), (int)HttpStatusCode.Created)]
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] Work obj)
        {
            try
            {
                Work? item = await _dbContext.Work
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == obj.Id);
                if (item is not null)
                {
                    _dbContext.Entry(obj).State = EntityState.Modified;
                }
                else
                {
                    _dbContext.Entry(obj).State = EntityState.Added;
                }

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
        /// Удаление работы
        /// </summary>
        /// <param name="id">идентификатор объекта</param>
        /// <returns>HTTP ответ</returns>
        /// <response code="200">Успешное удаление</response>
        /// <response code="404">Объект для удаления не найден (status quo)</response>
        /// <response code="409">Существует некаскадная связь (status quo)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Work? obj = _dbContext.Work.FirstOrDefault(x => x.Id == id);
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