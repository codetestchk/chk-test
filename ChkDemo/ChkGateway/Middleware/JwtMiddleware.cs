// NOTE:
// Largely copied/pasted Authentication stuff from here https://jasonwatmore.com/post/2019/10/11/aspnet-core-3-jwt-authentication-tutorial-with-example-api#authenticate-response-cs
//

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChkSDK.Services;
using ChkSDK.SettingsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChkGateway.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            _next = next;
            _jwtSettings = jwtSettings.Value;
        }


        // Note: method DI becuase of when in the startup order middleware is bound ( before service configuration binding )
        public async Task Invoke(HttpContext context, IMerchantService merchantService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                AttachMerchantToContext(context, token, merchantService);
            }

            await _next(context);
        }

        private void AttachMerchantToContext(HttpContext context, string token, IMerchantService merchantService)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var merchantGuid = Guid.Parse(jwtToken.Claims.First(x => x.Type == "merchant_id").Value);

                // attach merchant to context on successful jwt validation
                if(merchantService.MerchantIDExists(merchantGuid).Result)
                {
                    context.Items["MerchantID"] = merchantGuid;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attaching user context failed : {ex.Message}");
                // do nothing if jwt validation fails
                // Merchant is not attached to context so request won't have access to secure routes
                // it will fail at next step ( authoization step )
            }
        }
    }
}
