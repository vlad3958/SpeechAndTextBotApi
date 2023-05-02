using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.Bot.Types;

namespace Bot;

public class dbModel
{
   
   // [BsonId]
   // [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    public string Username { get; set; }
    public string UkrText { get; set; }
    public string EngText { get; set; }
    public string Image { get; set; }
  
    
}