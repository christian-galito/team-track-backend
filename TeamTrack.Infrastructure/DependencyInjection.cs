using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamTrack.Application.Interfaces;
using TeamTrack.Infrastructure.Persistence;
using TeamTrack.Infrastructure.Repositories;
using TeamTrack.Infrastructure.Services.CurrentUser;
using TeamTrack.Infrastructure.Services.Security;

namespace TeamTrack.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<TeamTrackDbContext>(options =>
               options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITeamTrackDbContext>(provider => provider.GetRequiredService<TeamTrackDbContext>());

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            services.Scan(scan => scan
               .FromAssemblies(
                    typeof(DependencyInjection).Assembly)
               .AddClasses(classes => classes.AssignableTo<IRepository>())
               .AsImplementedInterfaces()
               .WithScopedLifetime());

            return services;
        }
    }
}
