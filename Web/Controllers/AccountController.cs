using Asp.Versioning;
using Data;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "teacher")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        private readonly ZerdaContext _dbContext;

        private readonly Configurator _configurator;

        public AccountController(ILogger<AccountController> logger, ZerdaContext dbContext, Configurator configurator)
        {
            _logger = logger;
            _dbContext = dbContext;
            _configurator = configurator;
        }

        #region GET
        /// <summary>
        /// Метод на получение аккаунтов
        /// </summary>
        /// <param name="limit">количество записей (до 50)</param>
        /// <param name="offset">смещение относительно начала таблицы</param>
        /// <returns>список объектов</returns>
        /// <response code="200">Успех</response>
        [ProducesResponseType(typeof(IEnumerable<Account>), (int)HttpStatusCode.OK)]
        [HttpGet()]
        public async Task<IActionResult> Get(int limit = 50, int offset = 0)
        {
            return StatusCode(200, await _dbContext.Account
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(Math.Min(limit, 50))
                .ToListAsync());
        }
        #endregion

        #region POST
        /// <summary>
        /// Добавление аккаунта
        /// </summary>
        /// <response code="200">Не возвращается для этого метода</response>
        /// <response code="201">Успешное добавление</response>
        /// <response code="204">Попытка добавления дубликата (status quo)</response>
        /// <returns>Созданный объект</returns>
        [ProducesResponseType(typeof(Account), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Account obj)
        {
            try
            {
                _dbContext.Account.Entry(obj).State = EntityState.Added;
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, obj);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    _logger.LogWarning("Попытка добавления дубликата");
                    return StatusCode(204);
                }
                return StatusCode(500, "DbException");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost("token/{login}/{password}")]
        [AllowAnonymous]
        public async Task<IActionResult> Token(string login, string password)
        {
            Account account;
            ClaimsIdentity identity;

            try
            {
                (account, identity) = GetIdentity(login, password);
            }
            catch (ArgumentException)
            {
                return BadRequest(new { errorText = "Invalid creditionals" });
            }


            DateTime now = DateTime.UtcNow;
            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _configurator.JwtOptions.Issuer,
                audience: _configurator.JwtOptions.Audience,
                claims: identity.Claims,
                notBefore: now,                    
                expires: now.Add(TimeSpan.FromMinutes(_configurator.JwtOptions.Lifetime)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurator.JwtOptions.Key)), SecurityAlgorithms.HmacSha256)
            );

            var response = new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(jwt),
                username = identity.Name,
                role = account.Student is null ? "teacher" : "student"
            };

            return new JsonResult(response);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// Удаление аккаунта
        /// </summary>
        /// <param name="id">идентификатор объекта</param>
        /// <returns>HTTP ответ</returns>
        /// <response code="200">Успешное удаление</response>
        /// <response code="404">Объект для удаления не найден (status quo)</response>
        /// <response code="409">Существует некаскадная связь (status quo)</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Account? obj = _dbContext.Account.FirstOrDefault(x => x.Id == id);
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

        private (Account, ClaimsIdentity) GetIdentity(string login, string password)
        {                     
            Account? account = _dbContext.Account
                .AsNoTracking()
                .Include(x => x.Student)
                .AsEnumerable()
                .FirstOrDefault(x =>
                x.Login == login &&
                BCrypt.Net.BCrypt.Verify(password, x.PasswordHash)
            );

            if (account is null)
            {
                throw new ArgumentException("identity not found-");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, (account.Student is null) ? "teacher" : "student"),
            };                       
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return (account, claimsIdentity);
        }
    }
}