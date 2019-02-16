using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories
{
    public interface IWriteOnlyRepositoryBase<in T> 
        : IWriteOnlyRepository<T>, IDisposable
        where T : class, IDataModel
    {

    }

    internal abstract class WriteOnlyRepositoryBase<TDomain, TPersistence>
        : IWriteOnlyRepositoryBase<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelSql
    {
        private readonly ICrudOrm _crudOrm;
        protected IProvideSqliteConnection ProvideConnection { get; }

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        protected WriteOnlyRepositoryBase(IProvideSqliteConnection provideConnection, ICrudOrm crudOrm)
        {
            _crudOrm = crudOrm;
            ProvideConnection = provideConnection;
        }

        protected abstract Converter<TDomain, TPersistence> ConvertToPersistence { get; }

        public virtual async Task AddAsync(TDomain dataModel)
        {
            if (dataModel.IsInserted) return;
            var persistenceModel = ConvertToPersistence(dataModel);
            await _crudOrm.CreateAsync(persistenceModel).ConfigureAwait(false);
            dataModel.Id = persistenceModel.Id;
        }

        public virtual async Task UpdateAsync(TDomain dataModel)
        {
            if (dataModel.IsInserted.Not()) return;
            await _crudOrm.UpdateAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(TDomain dataModel)
        {
            if (dataModel.IsInserted.Not()) return;
            await _crudOrm.DeleteAsync(ConvertToPersistence(dataModel)).ConfigureAwait(false);
        }

        public void Dispose()
        {
            CompositeDisposable.Dispose();
        }
    }
}
