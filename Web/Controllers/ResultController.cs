using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using System.Runtime.InteropServices;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly ILogger<ResultController> _logger;

        private readonly ZerdaContext _dbContext;

        public ResultController(ILogger<ResultController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// Get results by student id тест
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <param name="workId">work Id</param>
        /// <param name="disciplineId">discipline Id</param>
        /// <param name="groupId">group Id</param>
        /// <param name="limit">count records to get (max 50)</param>
        /// <param name="offset">starting position relative to the beginning of the table</param>
        /// <returns>List of objects</returns>
        /// <response code="200">Success</response>
        [ProducesResponseType(typeof(IEnumerable<Result>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> GetByStudentId(
            int? studentId = null,
            int? workId = null,
            int? disciplineId = null,
            int? groupId = null,
            int limit = 50,
            int offset = 0
            )
        {
            return StatusCode(200, await _dbContext.Result
                .AsNoTracking()
                .Include(x => x.Work).ThenInclude(x => x.Discipline)
                .Include(x => x.Student).ThenInclude(x => x.Group)
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => disciplineId == null || x.Work.DisciplineId == disciplineId)
                .Where(x => groupId == null || x.Student.Group.Id == groupId)
                .OrderBy(x => x.StudentId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                { 
                    x.StudentId,
                    x.WorkId,
                    Tasks = UnsetBitsAfterN(x.Tasks, (uint)x.Work.TaskCount),
                    WorkTaskCount = x.Work.TaskCount
                })
                .ToListAsync()
            );
        }

        /// <summary>
        /// Get results by student id for kirill
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <param name="workId">work Id</param>
        /// <param name="disciplineId">discipline Id</param>
        /// <param name="groupId">group Id</param>
        /// <param name="limit">count records to get (max 50)</param>
        /// <param name="offset">starting position relative to the beginning of the table</param>
        /// <returns>List of objects</returns>
        /// <response code="200">Success</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpGet("withBitArray")]
        public async Task<IActionResult> GetByStudentIdBitArray(
            int? studentId = null,
            int? workId = null,
            int? disciplineId = null,
            int? groupId = null,
            int limit = 50,
            int offset = 0
            )
        {
            return StatusCode(200, await _dbContext.Result
                .AsNoTracking()
                .Include(x => x.Work).ThenInclude(x => x.Discipline)
                .Include(x => x.Student).ThenInclude(x => x.Group)
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => disciplineId == null || x.Work.DisciplineId == disciplineId)
                .Where(x => groupId == null || x.Student.Group.Id == groupId)
                .OrderBy(x => x.StudentId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                {
                    x.StudentId,
                    x.WorkId,
                    Tasks = BitArrayToIntArray(new BitArray(BitConverter.GetBytes(x.Tasks)), (uint)x.Work.TaskCount),
                    WorkTaskCount = x.Work.TaskCount
                })
                .ToListAsync()
            );
        }
        #endregion

        #region POST
        /// <summary>
        /// Setting tasks value
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="204">Success update</response>
        /// <response code="404">Couldn't create object before post (state unchanged)</response>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost("{studentId}/{workId}/{value}")]
        public async Task<IActionResult> Post([Required] int studentId, [Required] int workId, [Required] uint value)
        {
            try
            {
                await PostData(studentId, workId, value);
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or student");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Setting tasks value from body
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="204">Success update</response>
        /// <response code="404">Couldn't create object before post (state unchanged)</response>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost("{studentId}/{workId}")]
        public async Task<IActionResult> PostByJson([Required] int studentId, [Required] int workId, [FromBody] int[] values)
        {
            try
            {
                await PostData(studentId, workId, BitArrayToInt(
                    new BitArray(
                        values.Select(Convert.ToBoolean).ToArray())
                    )
                );
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or student");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task PostData(int studentId, int workId, uint value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkId == workId);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workId);

            if (result is null)
            {
                result = new Result()
                {
                    StudentId = studentId,
                    WorkId = workId,
                };

                _dbContext.Result.Add(result);
            }
            result.Tasks = value;
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region PUT
        /// <summary>
        /// Adding (logical disjunction) tasks value
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="204">Success update</response>
        /// <response code="404">Couldn't create object before put (state unchanged)</response>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]        
        [HttpPut("{studentId}/{workId}/{value}")]        
        public async Task<IActionResult> Put([Required] int studentId, [Required] int workId, [Required] ulong value)
        {
            try
            {
                await PutData(studentId, workId, value);
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or student");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Adding (logical disjunction) tasks value from body
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="204">Success update</response>
        /// <response code="404">Couldn't create object before put (state unchanged)</response>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPut("{studentId}/{workId}")]
        public async Task<IActionResult> PutByJson([Required] int studentId, [Required] int workId, [FromBody] int[] values)
        {
            try
            {                
                await PutData(studentId, workId, BitArrayToInt(
                    new BitArray(
                        values.Select(Convert.ToBoolean).ToArray())
                    )
                );
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or student");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task PutData(int studentId, int workId, ulong value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkId == workId);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workId);

            if (result is null)
            {
                result = new Result()
                {
                    StudentId = studentId,
                    WorkId = workId,
                };

                _dbContext.Result.Add(result);
            }
            result.Tasks |= value;
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region PATCH
        /// <summary>
        /// Setting task value by task number
        /// </summary>
        /// <response code="200">Never return</response>
        /// <response code="204">Success update</response>
        /// <response code="404">Couldn't create object before put (state unchanged)</response>
        /// <returns></returns>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpPatch("{studentId}/{workId}/{taskNumber}/{value}")]
        public async Task<IActionResult> Patch([Required] int studentId, [Required] int workId, [Required] int taskNumber, [Required] int value)
        {
            try
            {
                await PatchData(studentId, workId, taskNumber, value);
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or student");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task PatchData(int studentId, int workId, int bitN, int value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkId == workId);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workId);

            if (result is null)
            {
                result = new Result()
                {
                    StudentId = studentId,
                    WorkId = workId,
                };

                _dbContext.Result.Add(result);
            }
            if (Convert.ToBoolean(value))
            {
                result.Tasks |= ((ulong)1) << (bitN - 1);
            } 
            else
            {
                result.Tasks &= ~(((ulong)1) << (bitN - 1));
            }
            
            await _dbContext.SaveChangesAsync();
        }
        #endregion 

        #region DELETE
        /// <summary>
        /// Deleting object
        /// </summary>
        /// <param name="studentId">Student's id</param>
        /// <param name="workId">Work's id</param>
        /// <returns>response</returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{studentId}/{workId}")]
        public async Task<IActionResult> Delete(int studentId, int workId)
        {
            try
            {
                Result? obj = _dbContext.Result.FirstOrDefault(x => x.StudentId == studentId && x.WorkId == workId);
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
        #endregion

        #region Static

        private static uint BitArrayToInt(BitArray input)
        {
            uint result = 0;
            uint bitCount = 1;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i]) result |= bitCount;

                bitCount = bitCount << 1;
            }

            return result;
        }

        private static ulong UnsetBitsAfterN(ulong value, uint n)
        {
            return value & (ulong)((1 << (int)n) - 1);
        }

        private static string[] BitArrayToStringArray(BitArray bits, uint count)
        {
            List<string> builder = new List<string>();

            for (int i = 0; i < count; i++)
            {
                builder.Add(bits[i] ? "1" : "0");
            }

            return builder.ToArray();
        }

        private static int[] BitArrayToIntArray(BitArray bits, uint count)
        {
            List<int> builder = new List<int>();

            for (int i = 0; i < count; i++)
            {
                builder.Add(bits[i] ? 1 : 0);
            }

            return builder.ToArray();
        }
        #endregion
    }
}
