﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
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

        public static void ImportYNABTransactionsCSVToDB(string filePath, string filePathBudget)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath));
                        return;
                    }
                    Output.WriteLine("Starting to import YNAB transactions from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    List<YNAB.Transaction> transactions = new List<YNAB.Transaction>();
                    while (!streamReader.EndOfStream)
                    {
                        transactions.Add(streamReader.ReadLine());
                    }
                    YNAB.Transaction.ToOutput(transactions.Last());
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Output.WriteLine(string.Format("End of transaction import. Elapsed time was: {0}", elapsedTime));
                }
            }
            else
                Output.WriteLine(string.Format("The file of path '{0}' does not exist!", filePath));
            if (File.Exists(filePathBudget))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePathBudget, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.BudgetEntry.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePathBudget));
                        return;
                    }
                    Output.WriteLine("Starting to import YNAB budget entries from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    List<YNAB.BudgetEntry> budgetEntries = new List<YNAB.BudgetEntry>();
                    while (!streamReader.EndOfStream)
                    {
                        string nextLine = streamReader.ReadLine();
                        if (nextLine != "")
                            budgetEntries.Add(nextLine);
                    }
                    YNAB.BudgetEntry.ToOutput(budgetEntries.Last());
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Output.WriteLine(string.Format("End of budget entry import. Elapsed time was: {0}", elapsedTime));
                }
            }
            else
                Output.WriteLine(string.Format("The file of path '{0}' does not exist!", filePathBudget));
        }

    }
}
