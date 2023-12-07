using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "teacher")]
    public class ExportController : ControllerBase
    {
        private static readonly char delimeter = ',';

        private static readonly char delimeterReplace = ';';

        private readonly ILogger<ExportController> _logger;

        private readonly ZerdaContext _dbContext;

        public ExportController(ILogger<ExportController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        [HttpGet("{groupId}/{disciplineId}")]
        public async Task<IActionResult> Get(
            int groupId,
            int disciplineId,
            int? semesterId = null)
        {
            if (semesterId is null)
            {
                semesterId = (int)_dbContext.Semester.Max(x => x.StartYear + (x.IsSecond ? 0.5 : 0));
            }
            Group selectedGroup = _dbContext.Group.AsNoTracking().First(x => x.Id == groupId);
            StringBuilder builder = new StringBuilder();

            List<Work> assignedWorks = await _dbContext.Assignment
                .AsNoTracking()
                .Include(x => x.Work).ThenInclude(x => x.WorkType)
                .Where(x => x.GroupId == groupId).Select(x => x.Work)
                .ToListAsync();

            List<Student> groupStudents = await _dbContext.Student
                .Where(x => x.GroupId == groupId)
                .OrderBy(x => x.Surname).ThenBy(x => x.Name)
                .ToListAsync();

            IEnumerable<int> assignedWorksIds = assignedWorks.Select(x => x.Id);
            IEnumerable<int> groupStudentsIds = groupStudents.Select(x => x.Id);

            List<Result> results = await _dbContext.Result
                .Where(x => assignedWorksIds.Contains(x.WorkId) && groupStudentsIds.Contains(x.StudentId))
                .ToListAsync();

            builder.Append("STUDENT" + delimeter);
            foreach (Work work in assignedWorks)
            {
                builder.Append(
                    Secure(
                        GetBeatifyWorkTypeWithNumber(work.WorkType!, work.Number) 
                        + $" {work.Theme} ({work.TaskCount})"
                    ) + delimeter
                );
            }

            foreach (Student student in groupStudents)
            {
                builder.AppendLine();
                builder.Append(student.Surname + delimeter);
                foreach (Work work in assignedWorks)
                {
                    builder.Append(
                        GetBeatifyTasks(
                            results.FirstOrDefault(x => x.StudentId == student.Id && x.WorkId == work.Id)?.Tasks,
                            work.TaskCount
                        ) + delimeter
                    );
                }
            }

            return Ok(builder.ToString());
        }
        #endregion

        private static string GetBeatifyTasks(ulong? value, sbyte taskCount)
        {
            List<int> tasks = new List<int>();
            if (value.HasValue)
            {
                for (int i = 0; i < taskCount; i++)
                {
                    if ((value & (ulong)(1 << i)) != 0)
                    {
                        tasks.Add(i + 1);
                    }
                }
                return String.Join(delimeterReplace, tasks);
            }
            return String.Empty;
        }

        private static string GetBeatifyWorkTypeWithNumber(WorkType workType, int workNumber)
        {
            return String.Join(String.Empty, workType.Name.Split(' ').Select(x => x.Substring(0, 1)).ToList()) + workNumber;
        }       

        private static string Secure(string? value)
        {
            return value?.Replace(delimeter, delimeterReplace) ?? String.Empty;
        }
    }
}
