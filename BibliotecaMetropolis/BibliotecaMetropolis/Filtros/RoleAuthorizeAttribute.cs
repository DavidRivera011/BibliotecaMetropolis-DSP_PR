using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BibliotecaMetropolis.Filtros
{
    // Roles accesos
    // [RoleAuthorize("Administrador")]
    // [RoleAuthorize("Administrador","Usuario")]
    public class RoleAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;
            var session = http.Session;
            var token = session.GetString("JWToken");
            var sessionRole = session.GetString("Rol") ?? string.Empty;

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var config = http.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            if (config == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var jwtSettings = config.GetSection("Jwt");
            var keyString = jwtSettings["Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                context.Result = new ForbidResult();
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings["Issuer"]),
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings["Audience"]),
                    ValidAudience = jwtSettings["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                var claimRole = principal.Claims.FirstOrDefault(c => c.Type == "rol")?.Value ?? string.Empty;

                var effectiveRole = !string.IsNullOrEmpty(claimRole) ? claimRole : sessionRole;

                if (_roles.Length > 0)
                {
                    var allowed = _roles.Any(r => string.Equals(r, effectiveRole, StringComparison.OrdinalIgnoreCase));
                    if (!allowed)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }

                await next();
                return;
            }
            catch (SecurityTokenExpiredException)
            {
                // token expirado, redirigir a login
                http.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }
            catch (Exception)
            {
                // token inválido,redirigir a login
                http.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }
        }
    }
}
