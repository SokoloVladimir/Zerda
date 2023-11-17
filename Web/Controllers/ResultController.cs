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
        /// Get results by user id
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="workId">work Id</param>
        /// <param name="disciplineId">discipline Id</param>
        /// <param name="groupId">group Id</param>
        /// <param name="limit">count records to get (max 50)</param>
        /// <param name="offset">starting position relative to the beginning of the table</param>
        /// <returns>List of objects</returns>
        /// <response code="200">Success</response>
        [ProducesResponseType(typeof(IEnumerable<Result>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> GetByUserId(
            int? userId = null,
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
                .Include(x => x.User).ThenInclude(x => x.Group)
                .Where(x => userId == null || x.UserId == userId)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => disciplineId == null || x.Work.DisciplineId == disciplineId)
                .Where(x => groupId == null || x.User.Group.Id == groupId)
                .OrderBy(x => x.UserId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                { 
                    x.UserId,
                    x.WorkId,
                    Tasks = UnsetBitsAfterN(x.Tasks, (uint)x.Work.TaskCount),
                    WorkTaskCount = x.Work.TaskCount
                })
                .ToListAsync()
            );
        }

        /// <summary>
        /// Get results by user id for kirill
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="workId">work Id</param>
        /// <param name="disciplineId">discipline Id</param>
        /// <param name="groupId">group Id</param>
        /// <param name="limit">count records to get (max 50)</param>
        /// <param name="offset">starting position relative to the beginning of the table</param>
        /// <returns>List of objects</returns>
        /// <response code="200">Success</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpGet("withBitArray")]
        public async Task<IActionResult> GetByUserIdBitArray(
            int? userId = null,
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
                .Include(x => x.User).ThenInclude(x => x.Group)
                .Where(x => userId == null || x.UserId == userId)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => disciplineId == null || x.Work.DisciplineId == disciplineId)
                .Where(x => groupId == null || x.User.Group.Id == groupId)
                .OrderBy(x => x.UserId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                {
                    x.UserId,
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
        [HttpPost("{userId}/{workId}/{value}")]
        public async Task<IActionResult> Post([Required] int userId, [Required] int workId, [Required] uint value)
        {
            try
            {
                await PostData(userId, workId, value);
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or user");
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
        [HttpPost("{userId}/{workId}")]
        public async Task<IActionResult> PostByJson([Required] int userId, [Required] int workId, [FromBody] int[] values)
        {
            try
            {
                await PostData(userId, workId, BitArrayToInt(
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
                    return StatusCode(404, "Not such work or user");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task PostData(int userId, int workId, uint value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.UserId == userId && x.WorkId == workId);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workId);

            if (result is null)
            {
                result = new Result()
                {
                    UserId = userId,
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
        [HttpPut("{userId}/{workId}/{value}")]        
        public async Task<IActionResult> Put([Required] int userId, [Required] int workId, [Required] ulong value)
        {
            try
            {
                await PutData(userId, workId, value);
                return StatusCode(204);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("Cannot add or update a child row") == true)
                {
                    return StatusCode(404, "Not such work or user");
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
        [HttpPut("{userId}/{workId}")]
        public async Task<IActionResult> PutByJson([Required] int userId, [Required] int workId, [FromBody] int[] values)
        {
            try
            {                
                await PutData(userId, workId, BitArrayToInt(
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
                    return StatusCode(404, "Not such work or user");
                }
                return StatusCode(500, "DbUpdateException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private async Task PutData(int userId, int workId, ulong value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.UserId == userId && x.WorkId == workId);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workId);

            if (result is null)
            {
                result = new Result()
                {
                    UserId = userId,
                    WorkId = workId,
                };

                _dbContext.Result.Add(result);
            }
            result.Tasks |= value;
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Deleting object
        /// </summary>
        /// <param name="userId">User's id</param>
        /// <param name="workId">Work's id</param>
        /// <returns>response</returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{userId}/{workId}")]
        public async Task<IActionResult> Delete(int userId, int workId)
        {
            try
            {
                Result? obj = _dbContext.Result.FirstOrDefault(x => x.UserId == userId && x.WorkId == workId);
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
