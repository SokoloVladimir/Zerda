using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Web.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly ILogger<StudentController> _logger;

        private readonly ZerdaContext _dbContext;

        public StudentController(ILogger<StudentController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// ����� �� ��������� ���������
        /// </summary>
        /// <param name="groupId">���������� �� �������</param>
        /// <param name="limit">���������� ������� (�� 50)</param>
        /// <param name="offset">�������� ������������ ������ �������</param>
        /// <returns>������ ��������</returns>
        /// <response code="200">�����</response>
        [ProducesResponseType(typeof(IEnumerable<Student>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(
            int? groupId = null,
            int limit = 50,
            int offset = 0)
        {
            return StatusCode(200, await _dbContext.Student
                .Include(x => x.Account)
                .Include(x => x.Group)
                .Where(x => groupId == null || x.GroupId == groupId)
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }
        #endregion

        #region POST
        /// <summary>
        /// ���������� ��������
        /// </summary>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="201">�������� ����������</response>
        /// <response code="204">������� ���������� ��������� (status quo)</response>
        /// <returns>��������� ������</returns>
        [ProducesResponseType(typeof(Student), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Student obj)
        {
            try
            {
                await _dbContext.Student.AddAsync(obj);
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

        #region DELETE
        /// <summary>
        /// �������� ��������
        /// </summary>
        /// <param name="id">������������� �������</param>
        /// <returns>HTTP �����</returns>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="204">�������� ��������</response>
        /// <response code="404">������ ��� �������� �� ������ (status quo)</response>
        /// <response code="409">���������� ����������� ����� (status quo)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Student? obj = _dbContext.Student.FirstOrDefault(x => x.Id == id);
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
        #endregion
    }
}