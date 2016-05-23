using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace SecurityTest.Security
{
    public class ChangeRoleMessageHandler : DelegatingHandler
    {
        public const string Issuer = "corp";
        public const string Audience = "http://www.example.com";
        private const char AuthorizationHeaderSeparator = ':';
        private const char RoleSeparator = ',';
        private const int UsernameIndex = 0;
        private const int TokenIndex = 1;
        private const int UserRoleIndex = 2;
        private const int ExpectedCredentialCount = 3;
        private JwtAuthenticationService jwtSecurityService;

        public ChangeRoleMessageHandler()
        {
            jwtSecurityService = new JwtAuthenticationService();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!CanHandleAuthentication(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            string[] items = GetItem(request.Headers.Authorization);

            ClaimsPrincipal cprincipal = HttpContext.Current.User as ClaimsPrincipal;
            string rolesString = cprincipal.Claims.FirstOrDefault(p => p.Type == "Roles").Value;
            string userID = cprincipal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier).Value;
            string[] roles = rolesString.Split(RoleSeparator);
            string newToken;

            if (roles.Contains(items[UserRoleIndex]))
            {
                newToken = jwtSecurityService.CreateToken(userID, items[UserRoleIndex], items[UsernameIndex], Issuer, Audience);
            }
            else
            {
                return CreateChangeErrorResponse();
            }

            var response = new HttpResponseMessage();
            newToken = items[UsernameIndex] + ":" + newToken;
            jwtSecurityService.WriteTokenToResponse(response, newToken);

            return response;

        }

        /// <summary>
        /// 分离账号,Token和角色并获取
        /// </summary>
        /// <param name="authHeader"></param>
        /// <returns></returns>
        public string[] GetItem(AuthenticationHeaderValue authHeader)
        {
            var encodedCredentials = authHeader.Parameter;
            //var credentialBytes = Convert.FromBase64String(encodedCredentials);
            //var credentials = Encoding.ASCII.GetString(credentialBytes);
            var credentialParts = encodedCredentials.Split(AuthorizationHeaderSeparator);
            return credentialParts;
        }

        /// <summary>
        /// 检查能否处理此安全验证方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanHandleAuthentication(HttpRequestMessage request)
        {
            return (request.Headers != null
                    && request.Headers.Authorization != null
                    && request.Headers.Authorization.Scheme.ToLowerInvariant() == "change");
        }

        public HttpResponseMessage CreateChangeErrorResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Content = new StringContent("此用户无此角色，无法切换");
            return response;
        }
    }
}