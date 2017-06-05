using System;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.FastCrud;

namespace BFF.DB.Dapper
{
    public abstract class RepositoryBase<TDomain, TPersistance> : IRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistance : class, IPersistanceModel
    {
        private readonly IProvideConnection _provideConnection;

        public RepositoryBase(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        private void executeOnExistingOrNewConnection(Action<DbConnection> action, DbConnection connection = null)
        {
            if(connection != null) action(connection);
            else
            {
                using(TransactionScope transactionScope = new TransactionScope())
                using(connection = _provideConnection.Connection)
                {
                    action(connection);
                    transactionScope.Complete();
                }
            }
        }

        public virtual void CreateTable(DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Execute(CreateTableStatement), 
                connection);
        }

        public virtual void Add(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Insert(ConvertToPersistance(dataModel)), 
                connection);
        }

        public virtual void Update(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Update(ConvertToPersistance(dataModel)), 
                connection);
        }

        public virtual void Delete(TDomain dataModel, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                c => c.Delete(ConvertToPersistance(dataModel)), 
                connection);
        }

        public virtual TDomain Find(long id, DbConnection connection = null)
        {
            TDomain ret;
            if(connection != null) ret = ConvertToDomain(
                connection.Find<TPersistance>(
                    statement => statement.Where($"{nameof(IPersistanceModel.Id)}=@IdParam")
                                          .WithParameters(new {IdParam = id})).First());
            else
            {
                using(TransactionScope transactionScope = new TransactionScope())
                using(connection = _provideConnection.Connection)
                {
                    var result = connection.Find<TPersistance>(
                        statement => statement.Where($"{nameof(IPersistanceModel.Id)}=@IdParam")
                                              .WithParameters(new {IdParam = id})).First();
                    ret = ConvertToDomain(result);
                    transactionScope.Complete();
                }
            }
            return ret;
        }

        protected abstract Converter<TDomain, TPersistance> ConvertToPersistance { get; }
        protected abstract Converter<TPersistance, TDomain> ConvertToDomain { get; }
        protected abstract string CreateTableStatement { get; }
    }
}