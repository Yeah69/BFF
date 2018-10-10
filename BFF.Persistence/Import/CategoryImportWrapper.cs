using System.Collections.Generic;
using BFF.Persistence.Models;

namespace BFF.Persistence.Import
{
    public class CategoryImportWrapper
    {
        public Category Category { get; set; }

        public CategoryImportWrapper Parent { get; set; }

        public IList<CategoryImportWrapper> Categories { get; set; } = new List<CategoryImportWrapper>();

        public IList<IHaveCategory> TransAssignments { get; set; } = new List<IHaveCategory>();
    }
}