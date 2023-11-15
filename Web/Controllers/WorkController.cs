using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkController : AbstractController<Work>
    {
        public WorkController(ILogger<WorkController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }
    }
}