using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using Persistence = BFF.DB.PersistanceModels;
using Domain = BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public abstract class SubTransIncRepository <TDomain, TPersistence> : CachingRepositoryBase<TDomain, TPersistence> 
        where TDomain : class, Domain.ISubTransInc 
        where TPersistence : class, Persistence.IPersistanceModel
    {
        public SubTransIncRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        private string ParentalQuery => 
            $"SELECT * FROM [{typeof(TPersistence).Name}s] WHERE {nameof(Persistence.SubTransaction.ParentId)} = @ParentId;";
        
        public IEnumerable<TDomain> GetChildrenOf(long parentId, DbConnection connection = null) => 
            ConnectionHelper.QueryOnExistingOrNewConnection(
                c => c.Query<TPersistence>(ParentalQuery, new {ParentId = parentId})
                      .Select(sti => ConvertToDomain( (sti, c) )),
                ProvideConnection,
                connection);
    }
}