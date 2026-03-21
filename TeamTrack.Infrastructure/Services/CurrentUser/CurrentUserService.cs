using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TeamTrack.Application.Interfaces;

namespace TeamTrack.Infrastructure.Services.CurrentUser
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value ?? "0";

        public string? UserName =>
            _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("username")?
                .Value ?? "System User";

        public IEnumerable<string> Permissions =>
      
            _httpContextAccessor.HttpContext?
                .User
                ?.FindAll("Permission")
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
      
    }

}
