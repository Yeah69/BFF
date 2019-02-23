using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Persistence.Models.Import.YNAB;

namespace BFF.Persistence.Import
{
    internal interface IYnab4CsvImportContainer
    {
        void SetAccountStartingBalance(string name, long balance);

        void TrySetAccountStartingDate(string name, DateTime date);

        void AddAsTransfer(Ynab4Transaction ynab4Transaction);

        void AddAsTransaction(Ynab4Transaction ynab4Transaction);

        void AddParentAndSubTransactions(Ynab4Transaction parent, string parentMemo, IEnumerable<Ynab4Transaction> subTransactions);

        void AddBudgetEntry(Ynab4BudgetEntry budgetEntry);

        Task SaveIntoDatabase();
    }
}