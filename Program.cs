using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IATest.Models;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace ChatbotConsoleApp
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static TableClient tableClient;
        static IConfiguration Configuration;
        static string apiKey;

        static async Task Main(string[] args)
        {
            // Build configuration from appsettings.json
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            // Retrieve configurations
            apiKey = Configuration["OpenAI:ApiKey"];
            string connectionString = Configuration["AzureStorage:ConnectionString"];
            string tableName = Configuration["AzureStorage:TableName"];

            tableClient = new TableClient(connectionString, tableName);
            await tableClient.CreateIfNotExistsAsync();

            while (true)
            {
                Console.Write("You: ");
                string userInput = Console.ReadLine();

                if (userInput.ToLower() == "exit")
                {
                    break;
                }

                string response = await GetGpt3Response(userInput);
                Console.WriteLine($"Chatbot: {response}");

                // Record conversation in Azure Table Storage
                await AddConversationToTable(userInput, response);
            }
        }

        static async Task<string> GetGpt3Response(string userInput)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = userInput }
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            try
            {
                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                return responseObject.choices[0].message.content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing response: " + ex.Message);
                return "Error processing response from GPT-3.";
            }
        }

        static async Task AddConversationToTable(string userInput, string botResponse)
        {
            var conversation = new ConversationEntity
            {
                PartitionKey = "ChatPartition", // Can be replaced with a user identifier if needed
                RowKey = Guid.NewGuid().ToString(),
                UserInput = userInput,
                BotResponse = botResponse
            };

            await tableClient.AddEntityAsync(conversation);
        }
    }
}
