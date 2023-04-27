using System.Security.Claims;
using ASP_NET.Data;
using ASP_NET.Data.Entity;

namespace ASP_NET.Middleware;

public class SessionAuthMiddleware
{
    // формирование цепочки осуществляется путем того, что каждое звено вызывает следующее
    // сведения о последовательности формируются в Program.cs, а каждый объект получает
    // ссылку на следующее звено _next через конструктор

    private readonly RequestDelegate _next;

    // похоже на инжекцию, но это формирование цепочки - передача каждому звену ссылку на следующее
    public SessionAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
        ILogger<SessionAuthMiddleware> logger,
        DataContext dataContext)
    {
        // действия Middleware
        String? userId = context.Session.GetString("authUserId");
        if (userId is not null)
        {
            try
            {
                User? user = dataContext.Users.Find(Guid.Parse(userId));
                if (user is not null)
                {
                    // save
                    context.Items.Add("authUser", user);
                    Claim[] claims = new Claim[]
                    {
                        new Claim(ClaimTypes.Sid, userId),
                        new Claim(ClaimTypes.Name, user.RealName),
                        new Claim(ClaimTypes.NameIdentifier, user.Login),
                        new Claim(ClaimTypes.UserData, user.Avatar ?? String.Empty),
                        new Claim(ClaimTypes.Anonymous, user.EmailCode == null ? "false" : "true")
                    };
                    var principal = new ClaimsPrincipal(
                        new ClaimsIdentity(claims, nameof(SessionAuthMiddleware)));
                    context.User = principal;
                }
            }
            catch(Exception ex)
            {
                logger.LogWarning(ex, "SessionAuthMiddleware");
            }
        }
        
        
        logger.LogInformation("SessionAuthMiddleware works!");
        
        await _next(context);
    }
    
    
    // старая синхронная схема
    // public void Invoke(HttpContext context)
    // {
    //     _next(context);
    // }
}

// Класс-расширение, который позволит использовать инструкцию в app.UseSessionAuth

public static class SessionAuthMiddlewareExtension
{
    public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SessionAuthMiddleware>();
    }
}