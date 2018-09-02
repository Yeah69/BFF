using System;
using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class BudgetEntry : IPersistenceModel, IHaveCategory
    {
        [Key]
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }
    }
}