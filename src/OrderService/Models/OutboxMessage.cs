using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace OrderService.Models
{
    public class OutboxMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!; // JSON serializado do evento
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "pending"; // pending, processed, failed
        public int RetryCount { get; set; } = 0;
    }
}
