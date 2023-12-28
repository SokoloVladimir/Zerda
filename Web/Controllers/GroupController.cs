﻿using Asp.Versioning;
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
    [Authorize(Roles = "teacher")]
    public class GroupController : ControllerBase
    {
        private readonly ILogger<GroupController> _logger;

        private readonly ZerdaContext _dbContext;

        public GroupController(ILogger<GroupController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// Метод на получение групп
        /// </summary>
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType(typeof(IEnumerable<Group>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            return StatusCode(200, await _dbContext.Group
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }
        #endregion

        #region POST
        /// <summary>
        /// Добавление группы
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное добавление</response>
        /// <response code="204">Попытка добавления дубликата (status quo)</response>
        /// <returns>Созданный объект</returns>
        [ProducesResponseType(typeof(Group), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Group obj)
        {
            try
            {
                _dbContext.Group.Entry(obj).State = EntityState.Added;
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
        /// Обновление группы
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное обновление</response>
        /// <returns>обновленный объект</returns>
        [ProducesResponseType(typeof(Group), (int)HttpStatusCode.Created)]
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] Group obj)
        {
            try
            {
                Group? item = await _dbContext.Group
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
        /// Удаление группы
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
                Group? obj = _dbContext.Group.FirstOrDefault(x => x.Id == id);
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
