using Microsoft.EntityFrameworkCore;

namespace TeamTrack.Infrastructure.Interfaces
{
    public interface ISeeder
    {
        void Seed(ModelBuilder modelBuilder);
    }
}
