using BFF.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BFF.MongoDb
{
    public class MongoDbHelper
    {
        public static void testMongoDb()
        {
            Output.WriteLine("Connect...");
            IMongoClient client = new MongoClient("mongodb://localhost");
            IMongoDatabase database = client.GetDatabase("BFF");
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Catagories");



        }
    }
}
