using System;
using System.Collections.Generic;
using BFF.Persistence.Import.Models;
using MrMeeseeks.DataStructures;

namespace BFF.Persistence.Import
{
    internal class DtoImportContainer
    {
        public IReadOnlyList<TransactionDto> Transactions { get; set; } = new TransactionDto[0];
        public IReadOnlyList<TransferDto> Transfers { get; set; } = new TransferDto[0];
        public IReadOnlyList<ParentTransactionDto> ParentTransactions { get; set; } = new ParentTransactionDto[0];
        public IReadOnlyList<BudgetEntryDto> BudgetEntries { get; set; } = new BudgetEntryDto[0];
        public Forest<CategoryDto> Categories { get; set; } = new Forest<CategoryDto>();
        public IReadOnlyList<CategoryDto> IncomeCategories { get; set; } = new CategoryDto[0];
        public IReadOnlyDictionary<string, long> AccountStartingBalances { get; set; } = new Dictionary<string, long>();
        public IReadOnlyDictionary<string, DateTime> AccountStartingDates { get; set; } = new Dictionary<string, DateTime>();
    }
}