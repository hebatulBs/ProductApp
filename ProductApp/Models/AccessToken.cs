using System.Text.Json.Serialization;

namespace ProductApp.Models
{
    public class Result
    {
        [JsonPropertyName("accessToken")]
        public string accessToken { get; set; }
    }

    public class AccessToken
    {
        [JsonPropertyName("result")]
        public Result result { get; set; }
    }
}
