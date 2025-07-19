using System;

namespace WebApi.Database.Models
{
    public class UserDBO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Boolean IsAdmin { get; set; } = false;
        public Boolean IsRoot { get; set; } = false;
        public string UserName { get; set; } = string.Empty;
        public Boolean IsLockedOut { get; set; } = false;        

    }
}
