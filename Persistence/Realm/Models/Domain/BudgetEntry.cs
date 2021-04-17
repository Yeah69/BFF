using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class BudgetEntry : Model.Models.BudgetEntry, IRealmModel<Persistence.BudgetEntry>
    {
        private readonly IBudgetOrm _budgetOrm;
        private readonly IRealmTransRepository _transRepository;
        private readonly RealmObjectWrap<Persistence.BudgetEntry> _realmObjectWrap;

        public BudgetEntry(
            // parameters
            Persistence.BudgetEntry? realmObject,
            DateTime month,
            ICategory category, 
            long budget, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance,
            
            // dependencies
            ICrudOrm<Persistence.BudgetEntry> crudOrm,
            IBudgetOrm budgetOrm,
            IRealmTransRepository transRepository,
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory) 
            : base(
                month, 
                category, 
                budget,
                outflow, 
                balance, 
                aggregatedBudget, 
                aggregatedOutflow, 
                aggregatedBalance, 
                clearBudgetCache,
                updateBudgetCategory)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.BudgetEntry>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextBudgetEntryId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.BudgetEntry{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _budgetOrm = budgetOrm;
            _transRepository = transRepository;
            
            void UpdateRealmObject(Persistence.BudgetEntry ro)
            {
                ro.Category = 
                    (Category as Category)?.RealmObject
                        ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.MonthIndex = Month.ToMonthIndex();
                ro.Budget = Budget;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.BudgetEntry? RealmObject => _realmObjectWrap.RealmObject;

        public override Task InsertAsync()
        {
            return _realmObjectWrap.InsertAsync();
        }

        public override Task DeleteAsync()
        {
            return _realmObjectWrap.DeleteAsync();

        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransAsync()
        {
            return _transRepository.GetFromMonthAndCategoryAsync(Month, Category ?? throw new NullReferenceException("Shouldn't be null at that point"));
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync()
        {
            return _transRepository.GetFromMonthAndCategoriesAsync(
                Month, 
                Category.IterateTreeBreadthFirst(c => c.Categories));
        }

        protected override Task<long> AverageBudgetOfLastMonths(int monthCount)
        {
            return _budgetOrm.GetAverageBudgetOfLastMonths(
                this.Month.ToMonthIndex(), 
                (this.Category as Category)?.RealmObject ?? throw new InvalidCastException("Should be the Realm type, but isn't."),
                monthCount);
        }

        protected override Task<long> AverageOutflowOfLastMonths(int monthCount)
        {
            return _budgetOrm.GetAverageOutflowOfLastMonths(
                this.Month.ToMonthIndex(),
                (this.Category as Category)?.RealmObject ?? throw new InvalidCastException("Should be the Realm type, but isn't."),
                monthCount);
        }
    }
}
