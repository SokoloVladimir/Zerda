using Asp.Versioning;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        private readonly ZerdaContext _dbContext;

        public AccountController(ILogger<AccountController> logger, ZerdaContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        #region GET
        /// <summary>
        /// ����� �� ��������� ���������
        /// </summary>
        /// <param name="limit">���������� ������� (�� 50)</param>
        /// <param name="offset">�������� ������������ ������ �������</param>
        /// <returns>������ ��������</returns>
        /// <response code="200">�����</response>
        [ProducesResponseType(typeof(IEnumerable<Account>), (int)HttpStatusCode.OK)]
        [Authorize()]
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
        /// ���������� ��������
        /// </summary>
        /// <response code="200">�� ������������ ��� ����� ������</response>
        /// <response code="201">�������� ����������</response>
        /// <response code="204">������� ���������� ��������� (status quo)</response>
        /// <returns>��������� ������</returns>
        [ProducesResponseType(typeof(Account), (int)HttpStatusCode.Created)]
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Account obj)
        {
            try
            {
                await _dbContext.Account.AddAsync(obj);
                await _dbContext.SaveChangesAsync();
                return StatusCode(201, obj);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is MySqlConnector.MySqlException && ex.InnerException.Message.Contains("Duplicate entry"))
                {
                    _logger.LogWarning("������� ���������� ���������");
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
        public async Task<IActionResult> Token(string login, string password)
        {
            ClaimsIdentity? identity = GetIdentity(login, password);
            if (identity is null)
            {
                return BadRequest(new { errorText = "Invalid creditionals" });
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            return new JsonResult(response);
        }
        #endregion

        #region DELETE
        /// <summary>
        /// �������� ��������
        /// </summary>
        /// <param name="id">������������� �������</param>
        /// <returns>HTTP �����</returns>
        /// <response code="200">�������� ��������</response>
        /// <response code="404">������ ��� �������� �� ������ (status quo)</response>
        /// <response code="409">���������� ����������� ����� (status quo)</response>
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
                    _logger.LogWarning("������� �������� ��������� ������");
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

        private ClaimsIdentity? GetIdentity(string login, string password)
        {                     
            Account? account = _dbContext.Account
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault(x =>
                x.Login == login &&
                BCrypt.Net.BCrypt.Verify(password, x.PasswordHash)
            );

            if (account is null)
            {
                return null;
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, (account.Student is null) ? "teacher" : "student"),
            };
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return claimsIdentity;
        }
    }
}