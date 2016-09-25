using System.Collections.Generic;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.Helper.Import
{
    public class CategoryImportWrapper
    {
        public ICategory Category { get; set; }

        public CategoryImportWrapper Parent { get; set; }

        public IList<CategoryImportWrapper> Categories { get; set; } = new List<CategoryImportWrapper>();

        public IList<IHaveCategory> TitAssignments { get; set; } = new List<IHaveCategory>();
    }
}