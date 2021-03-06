﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MoreLinq;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Category : Model.Models.Category, IRealmModel<Persistence.Category>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmCategoryRepositoryInternal _repository;
        private readonly RealmObjectWrap<Persistence.Category> _realmObjectWrap;

        public Category(
            // parameters
            Persistence.Category? realmObject,
            string name, 
            ICategory? parent,
            
            // dependencies
            ICrudOrm<Persistence.Category> crudOrm,
            IMergeOrm mergeOrm,
            IRealmCategoryRepositoryInternal repository) : base(name, parent)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.Category>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextCategoryId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.Category{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(Persistence.Category ro)
            {
                ro.Parent =
                    Parent is null 
                        ? null 
                        : (Parent as Category)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.IsIncomeRelevant = false;
                ro.IncomeMonthOffset = 0;
                ro.Name = Name;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.Category? RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);

        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }

        public override async Task MergeToAsync(ICategory category)
        {
            if (!(category is Category)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));
            
            Categories
                .Join(category.Categories, c => c.Name, c => c.Name, (f, t) => f)
                .ForEach(f =>
                {
                    int i = -1;
                    // ReSharper disable once EmptyEmbeddedStatement => i gets iterated in the condition expression
                    while (category.Categories.Any(t => t.Name == $"{f.Name}{++i}")) ;
                    f.Name = $"{f.Name}{i}";
                });
            var categoryRealmObject = ((Category)category).RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point");
            await _mergeOrm.MergeCategoryAsync(
                RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"),
                categoryRealmObject)
                .ConfigureAwait(false);
            _repository.ClearCache();
            await _repository.ResetAll().ConfigureAwait(false);
            _repository.InitializeAll();
        }
    }
}
