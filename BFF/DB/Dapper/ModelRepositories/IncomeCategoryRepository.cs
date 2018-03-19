using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native;
using Category = BFF.DB.PersistenceModels.Category;

namespace BFF.DB.Dapper.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<MVVM.Models.Native.IIncomeCategory>
    {
        public override int Compare(MVVM.Models.Native.IIncomeCategory x, MVVM.Models.Native.IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<MVVM.Models.Native.IIncomeCategory>
    {
    }

    public sealed class IncomeCategoryRepository : ObservableRepositoryBase<MVVM.Models.Native.IIncomeCategory, Category>, IIncomeCategoryRepository
    {
        private readonly ICategoryOrm _categoryOrm;

        public IncomeCategoryRepository(IProvideConnection provideConnection, ICrudOrm crudOrm, ICategoryOrm categoryOrm)
            : base(provideConnection, crudOrm, new IncomeCategoryComparer())
        {
            _categoryOrm = categoryOrm;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(Category persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory(this,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<Category>> FindAllInner() => _categoryOrm.ReadIncomeCategoriesAsync();

        protected override Converter<MVVM.Models.Native.IIncomeCategory, Category> ConvertToPersistence => domainCategory =>
            new Category
            {
                Id = domainCategory.Id,
                Name = domainCategory.Name,
                ParentId = null,
                IsIncomeRelevant = true,
                MonthOffset = domainCategory.MonthOffset
            };
    }
}
