using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Web
{
    internal class AuthOptions
    {
        public const string ISSUER = "Zerda-issuer"; // издатель токена
        public const int LIFETIME = 1; 
        public const string AUDIENCE = "Zerda-audience"; // потребитель токена
        const string KEY = "5bIo*BD4ir&gna54njfdmkwar8C#";   // ключ для шифрации
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
