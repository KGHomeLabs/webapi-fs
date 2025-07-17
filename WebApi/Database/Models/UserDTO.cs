using System.Text.Json.Serialization;

namespace WebApi.Database.Models
{
    public class UserDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; }

        [JsonPropertyName("isRoot")]
        public bool IsRoot { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("isLockedOut")]
        public bool IsLockedOut { get; set; }
    }
}
