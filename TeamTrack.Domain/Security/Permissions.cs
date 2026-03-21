namespace TeamTrack.Domain.Security
{
    public class Permissions
    {
        //User 1-200
        public static class User
        {
            public static readonly PermissionDefinition Create = new() { Id = 1, Name = "CreateUser", Description = "Create a user" };
            public static readonly PermissionDefinition Read = new() { Id = 2, Name = "ReadUser", Description = "Read user details" };
            public static readonly PermissionDefinition Update = new() { Id = 3, Name = "UpdateUser", Description = "Update user details" };
            public static readonly PermissionDefinition Delete = new() { Id = 4, Name = "DeleteUser", Description = "Delete user details" };

            public const string CreatePolicy = "CreateUser";
            public const string ReadPolicy = "ReadUser";
            public const string UpdatePolicy = "UpdateUser";
            public const string DeletePolicy = "DeleteUser";

            public static readonly PermissionDefinition[] All = { Create, Read, Update, Delete };
            
        }

        //Project 201-400
        public static class Project
        {
            public static readonly PermissionDefinition Create = new() { Id = 201, Name = "CreateProject", Description = "Create a project" };
            public static readonly PermissionDefinition Read = new() { Id = 202, Name = "ReadProject", Description = "Read project details" };
            public static readonly PermissionDefinition Update = new() { Id = 203, Name = "UpdateProject", Description = "Update project details" };
            public static readonly PermissionDefinition Delete = new() { Id = 204, Name = "DeleteProject", Description = "Delete project details" };

            public const string CreatePolicy = "CreateProject";
            public const string ReadPolicy = "ReadProject";
            public const string UpdatePolicy = "UpdateProject";
            public const string DeletePolicy = "DeleteProject";

            public static readonly PermissionDefinition[] All = { Create, Read, Update, Delete };
        }

        private static readonly PermissionDefinition[][] _modules =
        {
            User.All,
            Project.All
        };

        public static IEnumerable<PermissionDefinition> All => _modules.SelectMany(x => x);
    }
}
