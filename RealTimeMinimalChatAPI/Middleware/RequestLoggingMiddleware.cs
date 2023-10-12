using RealTimeMinimalChatAPI.Data;
using RealTimeMinimalChatAPI.Models.Domain;
using System.Security.Claims;
using System.Text;

namespace RealTimeMinimalChatAPI.Middleware
{
    public class RequestLoggingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ApplicationDbContext _dbContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RequestLoggingMiddleware(RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task Invoke(HttpContext context)
        {

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var request = context.Request;
            var requestBody = await GetRequestBody(request);
            var currentUser = context.User;
            // Access user properties
            var userNameClaim = currentUser.FindFirst(ClaimTypes.Name);
            var userName = userNameClaim?.Value ?? "";
            var log = new Log
            {
                Username = userName,
                IpAddress = GetIpAddress(context),
                RequestBody = requestBody,
                Timestamp = DateTime.Now,
            };
            dbContext.Logs.Add(log);
            await dbContext.SaveChangesAsync();
            await _next(context);

        }
        private async Task<string> GetRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = string.Empty;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            return body;
        }
        private string GetIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
