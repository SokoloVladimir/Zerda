using Context.Data;
using Context.Model;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisciplineController : AbstractController<Discipline>
    {
        public DisciplineController(ILogger<DisciplineController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }
    }
}