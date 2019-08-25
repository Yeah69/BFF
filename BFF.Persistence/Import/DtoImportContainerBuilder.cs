using System;
using System.Collections.Generic;
using BFF.Persistence.Import.Models;
using MrMeeseeks.DataStructures;

namespace BFF.Persistence.Import
{
    internal class DtoImportContainer
    {
        public IReadOnlyList<TransactionDto> Transactions { get; set; }
        public IReadOnlyList<TransferDto> Transfers { get; set; }
        public IReadOnlyList<ParentTransactionDto> ParentTransactions { get; set; }
        public IReadOnlyList<BudgetEntryDto> BudgetEntries { get; set; }
        public Forest<CategoryDto> Categories { get; set; }
        public IReadOnlyList<CategoryDto> IncomeCategories { get; set; }
        public IReadOnlyDictionary<string, long> AccountStartingBalances { get; set; }
        public IReadOnlyDictionary<string, DateTime> AccountStartingDates { get; set; }
    }
}