using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BFF.Helper.Conversion
{
    class YNABConversion
    {
        public static void ImportYNABTransactionsCSVToDB(string filePath, string filePathBudget)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != Model.Conversion.YNAB.Transaction.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath));
                        return;
                    }
                    Output.WriteLine("Starting to import YNAB transactions from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    List<Model.Conversion.YNAB.Transaction> transactions = new List<Model.Conversion.YNAB.Transaction>();
                    while (!streamReader.EndOfStream)
                    {
                        transactions.Add(streamReader.ReadLine());
                    }
                    Model.Conversion.YNAB.Transaction.ToOutput(transactions.Last());
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
                    if (header != Model.Conversion.YNAB.BudgetEntry.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePathBudget));
                        return;
                    }
                    Output.WriteLine("Starting to import YNAB budget entries from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    List<Model.Conversion.YNAB.BudgetEntry> budgetEntries = new List<Model.Conversion.YNAB.BudgetEntry>();
                    while (!streamReader.EndOfStream)
                    {
                        string nextLine = streamReader.ReadLine();
                        if (nextLine != "")
                            budgetEntries.Add(nextLine);
                    }
                    Model.Conversion.YNAB.BudgetEntry.ToOutput(budgetEntries.Last());
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
