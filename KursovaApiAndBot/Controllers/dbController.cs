using Bot;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Telegram.Bot.Types;

namespace Kursova.Controllers;
[ApiController]
[Route("[controller]")]
public class dbController : Controller
{
   
    private readonly ILogger<dbController> _logger;

    public dbController(ILogger<dbController> logger)
    {
        _logger = logger;
    }
    public static string connectionString = "mongodb+srv://berezinv7930:root@cluster0.yklzpio.mongodb.net/?retryWrites=true&w=majority";
    public static string databaseName = "speechAndTextBot";
    public static string collectionName = "BotData";
    
  static  MongoClient client = new MongoClient(connectionString);
  static  IMongoDatabase db = client.GetDatabase(databaseName);
  static  IMongoCollection<dbModel> collection = db.GetCollection<dbModel>(collectionName);

    [HttpPost( "dbPost")]
    public async Task dbPost(string ukrText,string engText,string image, string username)
    {
        try
        {
            var personData = new dbModel
            { Username =username, UkrText =ukrText, EngText = engText,Image = image};
        
        await collection.InsertOneAsync(personData);
        
        var results = await collection.FindAsync(_ => true);
        foreach (var result in results.ToList())
        {
            Console.WriteLine($"{result.Id}: {result.Username} {result.UkrText} {result.EngText} {result.Image}");
        }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    [HttpDelete("dbDelete")]
    public async Task dbDelete(string id)
    {
      
      var objectId = ObjectId.Parse(id);
        var filter = Builders<dbModel>.Filter.Eq(m => m.Id, objectId);
        var result = await collection.DeleteOneAsync(filter);

    }
    
    
    
    /*
    [HttpPut("dbPut")]
    public async Task dbPut()
    {
        dbModel model = new dbModel();
        var filter = Builders<dbModel>.Filter.Eq(m => m.Id, model.Id);
        var update = Builders<dbModel>.Update
            .Set(m => m.UkrText, model.UkrText)
            .Set(m => m.EngText, model.EngText)
            .Set(m => m.Image, model.Image)
            .Set(m => m.Username, model.Username);

        var result = await collection.UpdateOneAsync(filter, update);


    }*/
}