using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IATest.Models;
using Microsoft.Extensions.Configuration;
using ChatbotConsoleApp.Models;
using ChatbotConsoleApp.Services;
using System.Collections.Generic;


namespace ChatbotConsoleApp
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static IConfiguration Configuration;
        static string apiKey;
        static TableStorageService tableStorageService; // Usa el servicio en lugar de TableClient

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

            tableStorageService = new TableStorageService(connectionString, tableName); // Inicializar servicio

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
            var recentMessages = tableStorageService.GetRecentMessages("ChatPartition");
            var messages = recentMessages.Select(m => new List<Message>
                {
                    new Message { Role = "user", Content = m.UserInput },
                    new Message { Role = "system", Content = m.BotResponse }
                })
                .SelectMany(m => m)
                .ToList();

            messages.Add(new Message { Role = "user", Content = userInput });

            var requestBody = new Gpt3Request
            {
                Messages = messages
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json"); 
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
                PartitionKey = "ChatPartition",
                RowKey = Guid.NewGuid().ToString(),
                UserInput = userInput,
                BotResponse = botResponse
            };

            await tableStorageService.AddConversationAsync(conversation);
        }
    }
}
