using Azure;
using Azure.Data.Tables;
using System;

namespace IATest.Models
{
    public class ConversationEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string UserInput { get; set; }
        public string BotResponse { get; set; }
    }
}
