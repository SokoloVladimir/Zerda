using Context.Data;
using Context.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Web.Controllers
{
    
    public abstract class AbstractController<T> : ControllerBase where T : class
    {
        protected readonly ILogger<AbstractController<T>> _logger;

        protected readonly ZerdaContext _dbContext;

        public AbstractController(ILogger<AbstractController<T>> logger, ZerdaContext dbContext)
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
        //[ProducesResponseType(typeof(List<Discipline>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        virtual public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            List<T> list = GetData<T>(limit, offset).Result!;
            return (list is not null) ? StatusCode(200, list) : StatusCode(404);
        }

        protected async Task<List<T>?> GetData<T>(int limit, int offset) where T : class
        {
            DbSet<T> dbSet = (DbSet<T>)_dbContext
                .GetType().GetProperties()
                .FirstOrDefault(p => p.Name == typeof(T).Name)!
                .GetValue(_dbContext)!;
            return await dbSet.Skip(offset).Take(limit).ToListAsync();
        }

        /// <summary>
        /// Adding
        /// </summary>
        /// <response code="200">Never return</response>\
        /// <response code="201">Success adding</response>
        /// <response code="204">Duplicate object (state unchanged)</response>
        /// <returns>Created object</returns>
        //[ProducesResponseType(_entity.GetType(), (int)HttpStatusCode.Created)]
        [HttpPost()]
        virtual public async Task<IActionResult> Post([FromBody] T obj)
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

        protected int DeleteData(T? obj)
        {
            try
            {            
                if (obj is null)
                {
                    return 404;
                }
                else
                {
                    _dbContext.Entry(obj).State = EntityState.Deleted;
                    _dbContext.SaveChangesAsync();
                    return 204;
                }
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException 
                    && ex.InnerException.Message.Contains("Cannot delete or update a parent row"))
                {
                    _logger.LogWarning("Попытка удаления связанной записи");
                    return 409;
                }
                return 500;
            }
            catch
            {
                return 500;
            }
            
        }
    }
}