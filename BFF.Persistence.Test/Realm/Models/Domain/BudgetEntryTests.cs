using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using NSubstitute;
using Realms;
using Xunit;
using BudgetEntry = BFF.Persistence.Realm.Models.Domain.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;

namespace BFF.Persistence.Test.Realm.Models.Domain
{
    public class BudgetEntryTests
    {
        [Fact]
        public void BudgetSetter_InsertedAndDifferentBudgetValue_CallToUpdateCache()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var budgetEntry = new Persistence.Realm.Models.Persistence.BudgetEntry()
            {
                Budget = 3L,
                Category = new Category { Name = "Test", IsIncomeRelevant = false },
                Month = month
            };
            var mockUpdateBudgetCache = Substitute.For<IUpdateBudgetCache>();
            var sut = new BudgetEntry(
                Substitute.For<ICrudOrm<Persistence.Realm.Models.Persistence.BudgetEntry>>(),
                mockUpdateBudgetCache,
                Substitute.For<IRealmTransRepository>(),
                Substitute.For<IRxSchedulerProvider>(),
                budgetEntry,
                budgetEntry.Month.LocalDateTime,
                Substitute.For<ICategory>(),
                budgetEntry.Budget,
                0L,
                0L);

            // Act
            sut.Budget = 23L;

            // Assert
            mockUpdateBudgetCache.Received().OnBudgetEntryChange(budgetEntry.Category, month);
        }

        [Fact]
        public void BudgetSetter_InsertedAndSameBudgetValue_DoesntCallToUpdateCache()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var budgetEntry = new Persistence.Realm.Models.Persistence.BudgetEntry()
            {
                Budget = 3L,
                Category = new Category { Name = "Test", IsIncomeRelevant = false },
                Month = month
            };
            var mockUpdateBudgetCache = Substitute.For<IUpdateBudgetCache>();
            var sut = new BudgetEntry(
                Substitute.For<ICrudOrm<Persistence.Realm.Models.Persistence.BudgetEntry>>(),
                mockUpdateBudgetCache,
                Substitute.For<IRealmTransRepository>(),
                Substitute.For<IRxSchedulerProvider>(),
                budgetEntry,
                budgetEntry.Month.LocalDateTime,
                Substitute.For<ICategory>(),
                budgetEntry.Budget,
                0L,
                0L);

            // Act
            sut.Budget = 3L;

            // Assert
            mockUpdateBudgetCache.DidNotReceive().OnBudgetEntryChange(budgetEntry.Category, month);
        }

        [Fact]
        public async Task DeleteAsync_Inserted_CallToUpdateCacheAsync()
        {
            // Arrange
            var fakeCategoryRealmObject = new Category {Name = "Test", IsIncomeRelevant = false};
            var category = new Persistence.Realm.Models.Domain.Category(
                Substitute.For<ICrudOrm<Category>>(),
                Substitute.For<IUpdateBudgetCache>(),
                Substitute.For<IMergeOrm>(),
                Substitute.For<IRealmCategoryRepositoryInternal>(),
                Substitute.For<IRxSchedulerProvider>(),
                fakeCategoryRealmObject,
                "Test",
                null);

            var fakeCrudOrm = Substitute.For<ICrudOrm<Persistence.Realm.Models.Persistence.BudgetEntry>>();
            fakeCrudOrm.CreateAsync(Arg.Any<Func<Realms.Realm,RealmObject>>()).Returns(Task.FromResult(true));

            var month = new DateTimeOffset(2023, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var budgetEntry = new Persistence.Realm.Models.Persistence.BudgetEntry()
            {
                Budget = 3L,
                Category = new Category { Name = "Test", IsIncomeRelevant = false },
                Month = month
            };
            var mockUpdateBudgetCache = Substitute.For<IUpdateBudgetCache>();
            var sut = new BudgetEntry(
                fakeCrudOrm,
                mockUpdateBudgetCache,
                Substitute.For<IRealmTransRepository>(),
                Substitute.For<IRxSchedulerProvider>(),
                budgetEntry,
                month.LocalDateTime,
                category,
                3L,
                0L,
                0L);

            // Act
            await sut.DeleteAsync().ConfigureAwait(false);

            // Assert
            await mockUpdateBudgetCache.Received().OnBudgetEntryChange(fakeCategoryRealmObject, month).ConfigureAwait(false);
        }
    }
}
