namespace WebApi
{
    public class UserDataService : IUserDataService
    {
        public string GetUserDisplayName(string userId)
        {
            // pretend real logic here
            return $"RealUser_{userId}";
        }
    }
}