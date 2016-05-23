using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SecurityTest.Models;
using MembershipTest.SecurityContext;


namespace SecurityTest.Security
{
    public class BasicAuthenticationMessageHandler : DelegatingHandler
    {
        private const char AuthorizationHeaderSeparator = ':';
        private const int UsernameIndex = 0;
        private const int PasswordIndex = 1;
        private const int ExpectedCredentialCount = 2;

        public BasicAuthenticationMessageHandler()
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            //if (HttpContext.Current.User.Identity.IsAuthenticated)
            //{
            //    return await base.SendAsync(request, cancellationToken);
            //}

            if (!CanHandleAuthentication(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            bool isAuthenticated;
            try
            {
                isAuthenticated = Authenticate(request);
            }
            catch (Exception e)
            {
                return CreateUnauthorizedResponse();
            }

            if (isAuthenticated)
            {

                //request.Headers.Authorization = new AuthenticationHeaderValue("bearer","");

                var response = await base.SendAsync(request, cancellationToken);

                //根据用户角色添加对应资源信息
                //response.Headers.Add("UserId", "21140211076");
                //response.Headers.Add("UserRole", "admin");
                //查询数据库获得用户角色对应前台显示资源信息
                //string str = "";
                User user = new User
                {
                    UserId = "123",
                    UserName = "123",
                    UserRole = "admin"
                };
                string str = JsonConvert.SerializeObject(user);
                response.Content = new StringContent(str, Encoding.GetEncoding("UTF-8"), "application/json");

                return response;
            }

            return CreateUnauthorizedResponse();
        }

        /// <summary>
        /// 检查能否处理此Basic安全验证方式
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanHandleAuthentication(HttpRequestMessage request)
        {
            return (request.Headers != null
                    && request.Headers.Authorization != null
                    && request.Headers.Authorization.Scheme.ToLowerInvariant() == "basic");
        }

        /// <summary>
        /// 用户登录验证
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool Authenticate(HttpRequestMessage request)
        {
            var authHeader = request.Headers.Authorization;
            if (authHeader == null)
            {
                return false;
            }

            var credentialParts = GetCredentialParts(authHeader);
            if (credentialParts.Length != ExpectedCredentialCount)
            {
                return false;
            }

            User user = GetUser(credentialParts[UsernameIndex]);

            if (user != null && credentialParts[PasswordIndex] == user.Password)
            {
                request.Headers.Add("UserId", user.UserId);
                request.Headers.Add("UserName", user.UserName);
                request.Headers.Add("UserRole", user.UserRole);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 根据用户名获取用户对象信息
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public virtual User GetUser(string username)
        {
            //无数据库时，构造模拟数据
            //if (username == "hanlin")
            //{
            //    return new User
            //    {
            //        UserId = "21140211076",
            //        UserName = "hanlin",
            //        UserRole = "admin",
            //        Password = "hanlin"
            //    };
            //}
            //if (username == "larry")
            //{
            //    return new User
            //    {
            //        UserId = "21140211077",
            //        UserName = "larry",
            //        UserRole = "expert",
            //        Password = "larry"
            //    };
            //}
            //else
            //    return null;

            var context = new SecurityContext();

            var membershipUser = context.MembershipUser.FirstOrDefault(m => m.UserName == username);

            var user = new User();

            if (membershipUser != null)
            {
                user.UserName = membershipUser.UserName;
                user.UserId = membershipUser.UserLoginId;
                user.Password = membershipUser.Password;
                user.UserRole = "admin";                        //暂时直接写入，应从数据库查询中获得
            }

            return user;
        }

        /// <summary>
        /// 分离账号密码并获取
        /// </summary>
        /// <param name="authHeader"></param>
        /// <returns></returns>
        public string[] GetCredentialParts(AuthenticationHeaderValue authHeader)
        {
            var encodedCredentials = authHeader.Parameter;
            //var credentialBytes = Convert.FromBase64String(encodedCredentials);
            //var credentials = Encoding.ASCII.GetString(credentialBytes);
            var credentialParts = encodedCredentials.Split(AuthorizationHeaderSeparator);
            return credentialParts;
        }

        /// <summary>
        /// 生成未授权Response
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage CreateUnauthorizedResponse()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("basic"));
            return response;
        }
    }
}