using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatbotConsoleApp.Models
{
    public class Gpt3Request
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }

        public Gpt3Request()
        {
            Model = "gpt-3.5-turbo";
            Messages = new List<Message>();
        }
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
