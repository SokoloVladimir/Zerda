using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : AbstractController<Result>
    {
        public ResultController(ILogger<ResultController> logger, ZerdaContext dbContext) : base(logger, dbContext) { }
    }
}
