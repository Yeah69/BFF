using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
using Dapper;
using Domain = BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{

    public interface ISubTransIncRepository<TDomain> : IRepositoryBase<TDomain> where TDomain : class, Domain.IDataModel
    {
        IEnumerable<TDomain> GetChildrenOf(long parentId, DbConnection connection = null);
    }

    public abstract class SubTransIncRepository <TDomain, TPersistence> : RepositoryBase<TDomain, TPersistence>, ISubTransIncRepository<TDomain>
        where TDomain : class, Domain.ISubTransInc 
        where TPersistence : class, IPersistenceModel
    {
        protected SubTransIncRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        private string ParentalQuery => 
            $"SELECT * FROM [{typeof(TPersistence).Name}s] WHERE {nameof(SubTransaction.ParentId)} = @ParentId;";
        
        public IEnumerable<TDomain> GetChildrenOf(long parentId, DbConnection connection = null) => 
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<TPersistence>(ParentalQuery, new {ParentId = parentId})
                      .Select(sti => ConvertToDomain( (sti, c) )),
                ProvideConnection,
                connection);
    }
}