namespace TeamTrack.Application.Features.Authentication.Responses
{
    public record RegisterUserResponse
    {
        public int UserId { get; init; }

        public string FirstName { get; init; }

        public string? MiddleName { get; init; }

        public string LastName { get; init; }

        public string UserName { get; init; }

        public string Email { get; init; }

        public RegisterUserResponse(int userId, string firstName, string? middleName, string lastName, string userName, string email)
        {
            UserId = userId;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            UserName = userName;
            Email = email;
        }
    }
}
