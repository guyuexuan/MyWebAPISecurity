using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Security.Tokens;
using System.Threading;
using System.Threading.Tasks;
using MembershipTest.SecurityContext;
using System.Net.Http.Headers;
using System.Text;

namespace SecurityTest.Security
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
         public const char AuthorizationHeaderSeparator = ':';
        private const int UsernameIndex = 0;
        private const int TokenIndex = 1;
        private const int ExpectedCredentialCount = 2;

        #region 属性
        public string UserName { get; set; }
        public string CookieNameToCheckForToken { get; set; }
        public SecurityToken SigningToken { get; set; }
        public string AllowedAudience { get; set; }
        public IEnumerable<string> AllowedAudiences { get; set; }
        public string Issuer { get; set; }
        public JwtAuthorizationService jwtAuthorizationService { get; set; }
        #endregion

        public JwtAuthorizationMessageHandler()
        {
            AllowedAudience = "http://www.example.com";
            Issuer = "corp";
            jwtAuthorizationService = new JwtAuthorizationService();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!CanHandleAuthentication(request))
            {
                return base.SendAsync(request, cancellationToken);
            }

            var tokenStringFromHeader = GetTokenStringFromHeader(request);
            var tokenStringFromCookie = GetTokenStringFromCookie(CookieNameToCheckForToken);
            var tokenString = tokenStringFromHeader ?? tokenStringFromCookie;

            if (string.IsNullOrEmpty(tokenString))
            {
                return base.SendAsync(request, cancellationToken);
            }

            JwtSecurityToken token;
            try
            {
                token = new JwtSecurityToken(tokenString);
            }
            catch (Exception ex)
            {
                return base.SendAsync(request, cancellationToken);
            }

            //使用SymmetricKey生成用于验证Token的SigningToken
            //string SymmetricKey = "cXdlcnR5dWlvcGFzZGZnaGprbHp4Y3Zibm0xMjM0NTY=";
            //SigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(SymmetricKey));
            //GetSigningToken(request);
            SigningToken = jwtAuthorizationService.GetSigningToken(request);
            
            if (SigningToken != null && token.SignatureAlgorithm != null)
            {
                if (token.SignatureAlgorithm.StartsWith("RS") && !(SigningToken is X509SecurityToken))
                {
                    return base.SendAsync(request, cancellationToken);
                }
                if (token.SignatureAlgorithm.StartsWith("HS") && !(SigningToken is BinarySecretSecurityToken))
                {
                    return base.SendAsync(request, cancellationToken);
                }
            }

            var parameters = new TokenValidationParameters
            {
                ValidAudience = AllowedAudience,
                IssuerSigningToken = SigningToken,
                ValidIssuer = Issuer,
                ValidAudiences = AllowedAudiences,
                //ValidateLifetime = true
            };

            try
            {

                IPrincipal principal = jwtAuthorizationService.TokenAuthorization(token, parameters);

                SetPrincipal(principal);
                //测试Principal中是否存在token中的Claim
                //ClaimsPrincipal cprincipal = principal as ClaimsPrincipal;
                //string Roles = cprincipal.Claims.FirstOrDefault(p => p.Type == "Roles").Value;
            }
            #region 异常处理
            catch (SecurityTokenExpiredException e)
            {
                var response = new HttpResponseMessage((HttpStatusCode)440)
                {
                    Content = new StringContent("Security token expired exception")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            catch (SecurityTokenSignatureKeyNotFoundException e)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Untrusted signing cert")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            catch (SecurityTokenInvalidAudienceException e)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Invalid token audience")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            catch (SecurityTokenValidationException e)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Invalid token")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            catch (SignatureVerificationFailedException e)
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Invalid token signature")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return tsc.Task;
            }
            catch (Exception e)
            {
                throw;
            }
            #endregion
            return base.SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// 从Request的Headers中获取TokenString
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual string GetTokenStringFromHeader(HttpRequestMessage request)
        {
            var authHeader = request.Headers.Authorization;
            if (authHeader == null) 
                return null;

            if (authHeader.Scheme == "Bearer" || authHeader.Scheme == "Change")
            {
                var credentialParts = jwtAuthorizationService.GetCredentialParts(authHeader);
                return credentialParts[TokenIndex];
            }            
            else
                return null;
        }

        /// <summary>
        /// 从Cookie中获取TokenString
        /// </summary>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        protected virtual string GetTokenStringFromCookie(string cookieName)
        {
            if (string.IsNullOrEmpty(cookieName))
                return null;
            var cookie = HttpContext.Current.Request.Cookies[cookieName];
            if (cookie == null)
                return null;
            else
            {
                var credentialParts = cookie.Value.Split(AuthorizationHeaderSeparator);
                return credentialParts[TokenIndex];
            }
        }

        public void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
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
                    && (request.Headers.Authorization.Scheme.ToLowerInvariant() == "change" || request.Headers.Authorization.Scheme.ToLowerInvariant() == "bearer"));
        }
    }
}