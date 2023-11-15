using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]    
    public class WorkTypeController : ControllerBase
    {
        private readonly ILogger<WorkTypeController> _logger;

        private readonly ZerdaContext _dbContext;

        public WorkTypeController(ILogger<WorkTypeController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Получение типа работы
        /// </summary>
        /// <response code="200">Успешное выполнение</response>
        /// <returns>Список типа работ</returns>
        //[Authorize(Roles = "teacher")]
        [ProducesResponseType(typeof(string[]), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IEnumerable<String>> Get()
        {
            return await _dbContext.WorkType.Select(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// Добавление типа работы
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>\
        /// <response code="201">Успешное добавление</response>
        /// <response code="204">Объект уже существует (состояние не изменено)</response>
        /// <returns>Созданный объект</returns>
        [ProducesResponseType(typeof(WorkType), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] WorkType obj)
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
        /// Удаление типа работы
        /// </summary>
        /// <param name="name">Наименование удаляемого типа работы</param>
        /// <returns></returns>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="204">Успешное удаление</response>
        /// <response code="404">Не удалось найти объект (состояние не изменено)</response>
        /// <response code="409">Не удалось удалить так как есть связанные записи (состояние не изменено)</response>
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(String name)
        { 
            try
            {
                WorkType? obj = _dbContext.WorkType.FirstOrDefault(x => x.Name == name);
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