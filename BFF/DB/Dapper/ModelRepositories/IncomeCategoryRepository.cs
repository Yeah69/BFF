using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using Category = BFF.DB.PersistenceModels.Category;

namespace BFF.DB.Dapper.ModelRepositories
{

    public class IncomeCategoryComparer : Comparer<IIncomeCategory>
    {
        public override int Compare(IIncomeCategory x, IIncomeCategory y) => 
            StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x?.Name, y?.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<IIncomeCategory>
    {
    }

    public sealed class IncomeCategoryRepository : ObservableRepositoryBase<IIncomeCategory, Category>, IIncomeCategoryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryOrm _categoryOrm;

        public IncomeCategoryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            ICategoryOrm categoryOrm)
            : base(provideConnection, crudOrm, new IncomeCategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryOrm = categoryOrm;
        }


        protected override Task<IIncomeCategory> ConvertToDomainAsync(Category persistenceModel)
        {
            return Task.FromResult<IIncomeCategory>(
                new IncomeCategory(this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.MonthOffset));
        }

        protected override Task<IEnumerable<Category>> FindAllInnerAsync() => _categoryOrm.ReadIncomeCategoriesAsync();

        protected override Converter<IIncomeCategory, Category> ConvertToPersistence => domainCategory =>
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
