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

        #region GET
        /// <summary>
        /// ����� �� ��������� ����� �����
        /// </summary>
        /// <param name="limit">���������� ������� (�� 50)</param>
        /// <param name="offset">�������� ������������ ������ �������</param>
        /// <returns>������ ��������</returns>
        /// <response code="200">�����</response>
        [ProducesResponseType(typeof(IEnumerable<WorkType>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            return StatusCode(200, await _dbContext.WorkType
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }
        #endregion

        #region POST
        /// <summary>
        /// ���������� ���� ������
        /// </summary>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="201">�������� ����������</response>
        /// <response code="204">������� ���������� ��������� (status quo)</response>
        /// <returns>��������� ������</returns>
        [ProducesResponseType(typeof(WorkType), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] WorkType obj)
        {
            try
            {
                _dbContext.WorkType.Entry(obj).State = EntityState.Added;
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
        #endregion

        #region PUT
        /// <summary>
        /// ���������� ���� ������
        /// </summary>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="201">�������� ����������</response>
        /// <returns>����������� ������</returns>
        [ProducesResponseType(typeof(WorkType), (int)HttpStatusCode.Created)]
        [HttpPut()]
        public async Task<IActionResult> Put([FromBody] WorkType obj)
        {
            try
            {
                WorkType? item = await _dbContext.WorkType
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
                    _logger.LogWarning("������� ���������� ���������");
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
        /// �������� ���� ������
        /// </summary>
        /// <param name="id">������������� �������</param>
        /// <returns>HTTP �����</returns>
        /// <response code="200">�������� ��������</response>
        /// <response code="404">������ ��� �������� �� ������ (status quo)</response>
        /// <response code="409">���������� ����������� ����� (status quo)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                WorkType? obj = _dbContext.WorkType.FirstOrDefault(x => x.Id == id);
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
        #endregion
    }
}