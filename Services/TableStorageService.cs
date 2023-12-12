using Azure.Data.Tables;
using IATest.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatbotConsoleApp.Services
{
    public class TableStorageService
    {
        private TableClient tableClient;

        public TableStorageService(string connectionString, string tableName)
        {
            tableClient = new TableClient(connectionString, tableName);
            tableClient.CreateIfNotExists();
        }

        public async Task AddConversationAsync(ConversationEntity conversation)
        {
            await tableClient.AddEntityAsync(conversation);
        }

        public List<ConversationEntity> GetRecentMessages(string partitionKey)
        {
            var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-10);
            var filter = $"PartitionKey eq '{partitionKey}' and Timestamp ge datetime'{tenMinutesAgo:o}'";

            // Execute the query asynchronously and retrieve all results
            var query = tableClient.Query<ConversationEntity>(filter);
            List<ConversationEntity> messages = new List<ConversationEntity>();

            foreach (var page in query.AsPages())
            {
                messages.AddRange(page.Values);
            }

            return messages;
        }
    }
}