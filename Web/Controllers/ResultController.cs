using Context.Data;
using Context.Model;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : AbstractController<Result>
    {
        public ResultController(ILogger<ResultController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }
    }
}
