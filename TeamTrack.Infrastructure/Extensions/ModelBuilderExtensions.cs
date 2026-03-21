using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TeamTrack.Infrastructure.Interfaces;

namespace TeamTrack.Infrastructure.Extensions
{
 
    public static class ModelBuilderExtensions
    {
        public static void ApplySeedersFromAssembly(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var seeders = assembly.GetTypes()
                .Where(t => typeof(ISeeder).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<ISeeder>();

            foreach (var seeder in seeders)
            {
                seeder.Seed(modelBuilder);
            }
        }
    }
}
