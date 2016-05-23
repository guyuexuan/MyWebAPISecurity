using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Collections;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using MembershipTest.SecurityContext;


namespace SecurityTest.Security
{
    public class JwtAuthenticationService
    {
        public JwtAuthenticationService()
        { }

        /// <summary>
        /// 产生随机密钥（由‘0-1’、‘a-z’和‘A-Z’随机生成）
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="length">所需密钥长度</param>
        /// <returns></returns>
        public string CreateSymmetricKey(string UserName, int length)
        {
            int number;
            char code;
            string key = string.Empty;
            byte[] bytes = new byte[4];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            Random rand = new Random(BitConverter.ToInt32(bytes, 0));

            for (int i = 0; i < length; i++)
            {
                number = rand.Next();
                if (number % 3 == 0)
                {
                    code = (char)('0' + (char)(number % 10));
                }
                else
                {
                    if (number % 3 == 1)
                    {
                        code = (char)('a' + (char)(number % 26));
                    }
                    else
                    {
                        code = (char)('A' + (char)(number % 26));
                    }
                }

                key += code.ToString();
            }

            var context = new SecurityContext();
            var membershipUser = context.MembershipUser.FirstOrDefault(m => m.UserName == UserName);
            membershipUser.SymmetricKey = key;
            context.SaveChanges();

            return key;
        }

        /// <summary>
        /// 由参数生成Token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userRole">用户登陆角色</param>
        /// <param name="symmetricKey"></param>
        /// <param name="issuer"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public string CreateToken(string userId, string userRole, string userName, string issuer, string audience)
        {
            //var key = Convert.FromBase64String(SymmetricKey);
            string symmetricKey = CreateSymmetricKey(userName, 20);

            var key = Encoding.ASCII.GetBytes(symmetricKey);

            var credentials = new SigningCredentials(
                new InMemorySymmetricSecurityKey(key),
                "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                "http://www.w3.org/2001/04/xmlenc#sha256");

            //过期时间
            //var expiration = DateTime.UtcNow.AddMinutes(1).ToLongTimeString();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim("Roles","admin,departmentmanger,expert")
                    //new Claim("exp", expiration)
                }),
                TokenIssuerName = issuer,
                AppliesToAddress = audience,
                SigningCredentials = credentials,
                //设置过期时间。
                Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        /// <summary>
        /// 将Token写入Response的Header中和Cookie中（Cookie功能尚未测试）
        /// </summary>
        /// <param name="response"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public HttpResponseMessage WriteTokenToResponse(HttpResponseMessage response, string token)
        {
            //把token信息添加到response中
            response.Headers.Add("Token", token);

            var cookie = new CookieHeaderValue("UserToken", token) { HttpOnly = true };
            //设置Cookie过期时间
            cookie.Expires = DateTimeOffset.Now.AddHours(1);
            //把token添加到Cookie中
            response.Headers.AddCookies(new CookieHeaderValue[] { cookie });

            return response;
        }
    }
}