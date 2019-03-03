using System.Collections.Generic;
using BFF.Persistence.Sql.Models.Persistence;

namespace BFF.Persistence.Import
{
    public class CategoryImportWrapper
    {
        public ICategorySql Category { get; set; }

        public CategoryImportWrapper Parent { get; set; }

        public IList<CategoryImportWrapper> Categories { get; set; } = new List<CategoryImportWrapper>();

        public IList<IHaveCategorySql> TransAssignments { get; set; } = new List<IHaveCategorySql>();
    }
}