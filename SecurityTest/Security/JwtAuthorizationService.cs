using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel.Security.Tokens;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MembershipTest.SecurityContext;


namespace SecurityTest.Security
{
    public class JwtAuthorizationService
    {
        private const char AuthorizationHeaderSeparator = ':';
        private const int UsernameIndex = 0;
        private const int TokenIndex = 1;
        private const int ExpectedCredentialCount = 3;
        public JwtAuthorizationService()
        { }
        
        public SecurityToken GetSigningToken(HttpRequestMessage request)
        {
            string symmetricKey = null;

            var authHeader = request.Headers.Authorization;
            if (authHeader == null)
            {
                return null;
            }
            var credentialParts = GetCredentialParts(authHeader);
            if (credentialParts.Length != ExpectedCredentialCount && credentialParts.Length != ExpectedCredentialCount - 1)
            {
                return null;
            }

            var context = new SecurityContext();
            string username = credentialParts[UsernameIndex];
            var user = context.MembershipUser.FirstOrDefault(u => u.UserName == username);

            if (user != null)
            {
                symmetricKey = user.SymmetricKey;
                return new BinarySecretSecurityToken(Encoding.ASCII.GetBytes(symmetricKey));
            }
            else
            {
                return null;
            }

        }

        public string[] GetCredentialParts(AuthenticationHeaderValue authHeader)
        {
            var encodedCredentials = authHeader.Parameter;
            //var credentialBytes = Convert.FromBase64String(encodedCredentials);
            //var credentials = Encoding.ASCII.GetString(credentialBytes);
            var credentialParts = encodedCredentials.Split(AuthorizationHeaderSeparator);
            return credentialParts;
        }

        public IPrincipal TokenAuthorization(JwtSecurityToken token, TokenValidationParameters parameters)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            IPrincipal principal = tokenHandler.ValidateToken(token.RawData, parameters, out validatedToken);
            //string expiration = token.Claims.FirstOrDefault(p => p.Type == "exp").Value;
            var exp = Convert.ToDateTime(token.ValidTo);
            if (DateTime.UtcNow > exp)
            {
                throw new SecurityTokenExpiredException();
            }

            return principal;
        }
    }
}