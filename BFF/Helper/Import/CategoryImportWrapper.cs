using System.Collections.Generic;
using BFF.DB.PersistenceModels;

namespace BFF.Helper.Import
{
    public class CategoryImportWrapper
    {
        public Category Category { get; set; }

        public CategoryImportWrapper Parent { get; set; }

        public IList<CategoryImportWrapper> Categories { get; set; } = new List<CategoryImportWrapper>();

        public IList<IHaveCategory> TransAssignments { get; set; } = new List<IHaveCategory>();
    }
}