using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Cryptography;
using MembershipTest.SecurityContext;

namespace SecurityTest.Security
{
    public class JwtAuthenticationMessageHandler : DelegatingHandler
    {
        public const string Issuer = "corp";
        public const string Audience = "http://www.example.com";
        public JwtAuthenticationService jwtAuthenticationService;

        public JwtAuthenticationMessageHandler()
        {
            jwtAuthenticationService = new JwtAuthenticationService();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!CanHandleAuthentication(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            string UserId = "";
            string UserRole = "";
            string UserName = "";

            UserId = GetItemFromHeader(request, "UserId");
            UserRole = GetItemFromHeader(request, "UserRole");
            UserName = GetItemFromHeader(request, "UserName");

            string token = jwtAuthenticationService.CreateToken(UserId, UserRole, UserName, Issuer, Audience);

            var response = await base.SendAsync(request, cancellationToken);

            //新token由UserName和token构成，方便验证时取得用户所对应密钥。
            string newToken = UserName + ":" + token;
            response = jwtAuthenticationService.WriteTokenToResponse(response, newToken);
            response.StatusCode = HttpStatusCode.OK;

            return response;
        }

        /// <summary>
        /// 从HttpRequestMessage的Headers中获取自定义的Item
        /// </summary>
        /// <param name="request">HTTP请求</param>
        /// <param name="itemname">自定义的Item的Name</param>
        /// <returns>获取的自定义的Item的Value</returns>
        public string GetItemFromHeader(HttpRequestMessage request, string itemname)
        {

            IEnumerable<string> values;
            string itemvalue = null;
            if (request.Headers.Contains(itemname))
            {
                //foreach (string HeaderKey in request.Headers.GetValues("UserId"))
                //{
                //    if (HeaderKey != null)
                //        UserId = HeaderKey;
                //}
                values = request.Headers.GetValues(itemname);
                IEnumerator i = values.GetEnumerator();
                if (i.MoveNext())
                {
                    itemvalue = i.Current.ToString();
                }
            }
            return itemvalue;
        }

        public bool CanHandleAuthentication(HttpRequestMessage request)
        {
            return (request.Headers != null
                    && request.Headers.Authorization != null
                    && request.Headers.Authorization.Scheme.ToLowerInvariant() == "basic");
        }
    }
}