using Azure.Data.Tables;
using IATest.Models; // Importa el namespace de tu modelo
using System;
using System.Threading.Tasks;

public class TableStorageService
{
    private TableClient tableClient;

    public TableStorageService(string connectionString, string tableName)
    {
        tableClient = new TableClient(connectionString, tableName);
        tableClient.CreateIfNotExists();
    }

    public async Task AddConversationAsync(string partitionKey, string userInput, string botResponse)
    {
        var conversation = new ConversationEntity
        {
            PartitionKey = partitionKey,
            RowKey = Guid.NewGuid().ToString(),
            UserInput = userInput,
            BotResponse = botResponse
        };

        await tableClient.AddEntityAsync(conversation);
    }

}
