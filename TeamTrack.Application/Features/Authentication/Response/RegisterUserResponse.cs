namespace TeamTrack.Application.Features.Authentication.Response
{
    public record RegisterUserResponse
    {
        public int UserId { get; private set; }

        public string FirstName { get; private set; }

        public string? MiddleName { get; private set; }

        public string LastName { get; private set; }

        public string UserName { get; private set; }

        public string Email { get; private set; }

        public RegisterUserResponse(int userId, string firstName, string? middleName, string lastName, string userName, string email)
        {
            UserId = userId;
            UserName = userName;
            Email = email;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
        }
    }
}
