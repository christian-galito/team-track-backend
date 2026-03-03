namespace TeamTrack.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public string EntityName { get; }
        
        public object? Key { get; }

        public NotFoundException(string entityName, object? key = null)
           : base($"{entityName} with key ({key}) was not found.")
        {
            EntityName = entityName;
            Key = key;
        }

        public NotFoundException(string entityName, params object[] keys)
            : base($"{entityName} with key ({string.Join(", ", keys)}) was not found.")
        {
            EntityName = entityName;
            Key = keys;
        }

        public NotFoundException(string entityName, System.Collections.IEnumerable keys)
            : base($"{entityName} with key ({FormatEnumerable(keys)}) was not found.")
        {
            EntityName = entityName;
            Key = keys;
        }

        private static string FormatEnumerable(System.Collections.IEnumerable enumerable)
        {
            var values = new List<string>();

            foreach (var item in enumerable)
            {
                values.Add(item?.ToString() ?? "null");
            }

            return string.Join(", ", values);
        }

    }
}
