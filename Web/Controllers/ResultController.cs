using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Net;

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
        /// Метод на получение результатов
        /// </summary>
        /// <param name="studentId">фильтрация по студенту</param>
        /// <param name="workId">фильтрация по работе</param>
        /// <param name="groupId">фильтрация по группе</param>
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType(typeof(IEnumerable<Result>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> GetByStudentId(
            int? studentId = null,
            int? workId = null,
            int? groupId = null,
            int limit = 50,
            int offset = 0
            )
        {
            return StatusCode(200, await _dbContext.Result
                .AsNoTracking()
                .Include(x => x.Work)
                .Include(x => x.Student)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => groupId == null || x.Student.GroupId == groupId)
                .OrderBy(x => x.StudentId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                {
                    x.StudentId,
                    x.WorkId,
                    Tasks = UnsetBitsAfterN(x.Tasks, (uint)x.Work.TaskCount),
                    TaskCount = x.Work.TaskCount
                })
                .ToListAsync()
            );
        }

        /// <summary>
        /// Метод на получение результатов как массив бит
        /// </summary>
        /// <param name="studentId">фильтрация по студенту</param>
        /// <param name="workId">фильтрация по работе</param>
        /// <param name="groupId">фильтрация по группе</param>
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpGet("withBitArray")]
        public async Task<IActionResult> GetByStudentIdBitArray(
            int? studentId = null,
            int? workId = null,
            int? groupId = null,
            int limit = 50,
            int offset = 0
            )
        {
            return StatusCode(200, await _dbContext.Result
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Work)                                
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => workId == null || x.WorkId == workId)
                .Where(x => groupId == null || x.Student.GroupId == groupId)
                .OrderBy(x => x.StudentId).ThenBy(x => x.WorkId)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .Select(x => new
                {
                    x.StudentId,
                    x.WorkId,
                    Tasks = BitArrayToIntArray(new BitArray(BitConverter.GetBytes(x.Tasks)), (uint)x.Work.TaskCount),
                    TaskCount = x.Work.TaskCount
                })
                .ToListAsync()
            );
        }
        #endregion

        #region POST
        /// <summary>
        /// Установка значения результата
        /// </summary>      
        /// <remarks>
        /// Точно устанавливает значения бит заданий. Биты отсчитываются от младшего (1 задание) к старшему. Активный бит означает выполненное задание.
        /// </remarks>
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <param name="value">значения бит заданий упакованные в ULong число</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное обновление</response>
        /// <response code="404">Не найден родительский объект (status quo)</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPost("{studentId}/{workId}/{value}")]
        public async Task<IActionResult> Post([Required] int studentId, [Required] int workId, [Required] uint value)
        {
            try
            {
                await PostData(studentId, workId, value);
                return StatusCode(200);
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
        /// Установка значения результата из тела
        /// </summary>
        /// <remarks>
        /// Точно устанавливает значения бит заданий. За счёт этого использовать с осторожностью.
        /// Биты отсчитываются от младшего (1 задание) к старшему. Активный бит означает выполненное задание.         
        /// </remarks>
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное обновление</response>
        /// <response code="404">Не найден родительский объект (status quo)</response>
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
                return StatusCode(200);
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

        private async Task PostData(int studentId, int workid, uint value)
        {
            Result? result = await _dbContext.Result.FirstOrDefaultAsync(x => x.StudentId == studentId && x.WorkId == workid);
            Work work = await _dbContext.Work.FirstAsync(x => x.Id == workid);

            if (result is null)
            {
                result = new Result()
                {
                    StudentId = studentId,
                    WorkId = workid,
                };

                _dbContext.Result.Add(result);
            }
            result.Tasks = value;
            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region PUT
        /// <summary>
        /// Добавление к результату значения
        /// </summary>      
        /// <remarks>
        /// Применяет к результату заданий дизъюнкцию (логическое сложение) отправленного значения. Безопасный метод. 
        /// </remarks>       
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <param name="value">значения бит заданий упакованные в ULong число</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное обновление</response>
        /// <response code="404">Не найден родительский объект (status quo)</response>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [HttpPut("{studentId}/{workId}/{value}")]
        public async Task<IActionResult> Put([Required] int studentId, [Required] int workId, [Required] ulong value)
        {
            try
            {
                await PutData(studentId, workId, value);
                return StatusCode(200);
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
        /// Добавление к результату значения из тела
        /// </summary>      
        /// <remarks>
        /// Применяет к результату заданий дизъюнкцию (логическое сложение) отправленного значения. Безопасный метод.
        /// </remarks>       
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <param name="values">массив значений</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное обновление</response>
        /// <response code="404">Не найден родительский объект (status quo)</response>
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
                return StatusCode(200);
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
        /// Установка состояния конкретного задания
        /// </summary>      
        /// <remarks>
        /// Выбивает или активирует соответствующий бит. Безопасный метод. 
        /// </remarks>
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <param name="taskNumber">номер задания (от 1)</param>
        /// <param name="value">значение 0/1</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное обновление</response>
        /// <response code="404">Не найден родительский объект (status quo)</response>
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [HttpPatch("{studentId}/{workId}/{taskNumber}/{value}")]
        public async Task<IActionResult> Patch([Required] int studentId, [Required] int workId, [Required] int taskNumber, [Required] int value)
        {
            try
            {
                await PatchData(studentId, workId, taskNumber, value);
                return StatusCode(200);
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
                result.Tasks |= (ulong)1 << bitN - 1;
            }
            else
            {
                result.Tasks &= ~((ulong)1 << bitN - 1);
            }

            await _dbContext.SaveChangesAsync();
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Удаление результата
        /// </summary>          
        /// <param name="studentId">студент</param>
        /// <param name="workId">работа</param>
        /// <returns>ответ</returns>
        /// <response code="200">Успешное удаление</response>
        /// <response code="404">Объект для удаления не найден (status quo)</response>
        /// <response code="409">Существует некаскадная связь (status quo)</response>
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
