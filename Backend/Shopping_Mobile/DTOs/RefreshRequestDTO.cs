
using Shopping_Mobile.DTOs;
using System.Text.Json.Serialization;

namespace Shopping_Mobile.DTOs
{
    public class RefreshRequestDTO
    {
        [JsonPropertyName("refreshToken")]
        public required string RefreshToken { get; set; }
    }
}
