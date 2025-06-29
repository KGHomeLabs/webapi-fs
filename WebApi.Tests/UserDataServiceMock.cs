namespace WebApi.Tests
{
    public class UserDataServiceMock : IUserDataService
    {
        public string GetUserDisplayName(string userId)
        {
            // Pretend this talks to a database
            return userId switch
            {
                "123test" => "Bob",
                _ => "Unknown"
            };
        }
    }
}