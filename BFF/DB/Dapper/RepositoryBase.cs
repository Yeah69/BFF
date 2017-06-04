using System;
using System.Data.Common;
using System.Linq;
using System.Transactions;
using BFF.DB.PersistanceModels;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.FastCrud;
using JetBrains.dotMemoryUnit.Util;

namespace BFF.DB.Dapper.ModelRepositories
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

        public virtual void CreateTable()
        {
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection connection = _provideConnection.Connection)
            {
                connection.Execute(CreateTableStatement);
                transactionScope.Complete();
            }
        }

        public virtual void Add(TDomain dataModel)
        {
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection connection = _provideConnection.Connection)
            {
                connection.Insert(ConvertToPersistance(dataModel));
                transactionScope.Complete();
            }
        }

        public virtual void Update(TDomain dataModel)
        {
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection connection = _provideConnection.Connection)
            {
                connection.Update(ConvertToPersistance(dataModel));
                transactionScope.Complete();
            }
        }

        public virtual void Delete(TDomain dataModel)
        {
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection connection = _provideConnection.Connection)
            {
                connection.Delete(ConvertToPersistance(dataModel));
                transactionScope.Complete();
            }
        }

        public virtual TDomain Find(long id)
        {
            TDomain ret;
            using(TransactionScope transactionScope = new TransactionScope())
            using(DbConnection connection = _provideConnection.Connection)
            {
                var result = connection.Find<TPersistance>(
                    statement => statement.Where($"{nameof(IPersistanceModel.Id)}=@IdParam")
                                          .WithParameters(new {IdParam = id})).First();
                ret = ConvertToDomain(result);
                transactionScope.Complete();
            }
            return ret;
        }

        protected abstract Converter<TDomain, TPersistance> ConvertToPersistance { get; }
        protected abstract Converter<TPersistance, TDomain> ConvertToDomain { get; }
        protected abstract string CreateTableStatement { get; }
    }
}