using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisciplineController : AbstractController<Discipline>
    {
        public DisciplineController(ILogger<DisciplineController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }

        /// <summary>
        /// Deleting
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        /// <response code="200">Never return</response>
        /// <response code="204">Success delete</response>
        /// <response code="404">Couldn't find obj (state unchanched)</response>
        /// <response code="409">Couldn't delete relationship (state unchanched)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return StatusCode(DeleteData(_dbContext.Discipline.FirstOrDefault(x => x.Id == id)));
        }
    }
}