using System.IO;
using System.Windows.Data;
using BFF.Helper;
using MongoDB.Driver;
using YNAB = BFF.Model.Conversion.YNAB;

namespace BFF.MongoDb
{
    public class MongoDbHelper
    {
        private static string clientConnectionName = "mongodb://localhost";
        private static string dataBaseName = "BFF_Test";

        private static IMongoClient client;
        private static IMongoDatabase database;

        public static void ConnectToMongoDb(string clientConnectionName, string dataBaseName)
        {
            clientConnectionName = (string.IsNullOrEmpty(clientConnectionName))
                ? MongoDbHelper.clientConnectionName
                : clientConnectionName;
            dataBaseName = (string.IsNullOrEmpty(dataBaseName)) ? MongoDbHelper.dataBaseName : dataBaseName;

            Output.WriteLine(string.Format("Connect to '{0}' ...", clientConnectionName));
            client = client ?? new MongoClient(clientConnectionName);
            Output.WriteLine(string.Format("Getting database '{0}' ...", dataBaseName));
            database = database ?? client.GetDatabase(dataBaseName);
            Output.WriteLine("Connected!");



            //IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Catagories");
            //Output.WriteLine(collection.CollectionNamespace.CollectionName);

        }

        public static void ImportYNABTransactionsCSVToDB(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CSVHeader)
                    {
                        Output.WriteLine("The file of path '{0}' is not a valid YNAB transactions CSV.");
                        return;
                    }
                    while (!streamReader.EndOfStream)
                    {
                        string currentLine = streamReader.ReadLine();
                        string[] entries = currentLine.Split();

                    }
                }
            }
            else
                Output.WriteLine(string.Format("The file of path '{0}' does not exist!", filePath));
        }

    }
}
