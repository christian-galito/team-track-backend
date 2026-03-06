using System.Text;
using TeamTrack.Domain.Common;

namespace TeamTrack.Infrastructure.Persistence
{
    public static class NamingConventions
    {
        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && input[i - 1] != '_')
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        public static string SoftDeleteFilter()
        {
            var columnName = ToSnakeCase(nameof(BaseEntity.IsDeleted));
            return $"\"{columnName}\" = false";
        }
    }
}

