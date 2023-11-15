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
        /// ��������� ���� ������
        /// </summary>
        /// <response code="200">�������� ����������</response>
        /// <returns>������ ���� �����</returns>
        //[Authorize(Roles = "teacher")]
        [ProducesResponseType(typeof(string[]), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IEnumerable<String>> Get()
        {
            return await _dbContext.WorkType.Select(x => x.Name).ToListAsync();
        }

        /// <summary>
        /// ���������� ���� ������
        /// </summary>
        /// <response code="200">�� ������������ ��� ����� ������</response>\
        /// <response code="201">�������� ����������</response>
        /// <response code="204">������ ��� ���������� (��������� �� ��������)</response>
        /// <returns>��������� ������</returns>
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
        /// �������� ���� ������
        /// </summary>
        /// <param name="name">������������ ���������� ���� ������</param>
        /// <returns></returns>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="204">�������� ��������</response>
        /// <response code="404">�� ������� ����� ������ (��������� �� ��������)</response>
        /// <response code="409">�� ������� ������� ��� ��� ���� ��������� ������ (��������� �� ��������)</response>
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