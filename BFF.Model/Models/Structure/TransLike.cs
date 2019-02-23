using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface ITransLike : IDataModel
    {
        string Memo { get; set; }
    }

    internal abstract class TransLike<TDomain, TPersistence> : DataModel<TDomain, TPersistence>, ITransLike
        where TDomain : class, ITransLike
        where TPersistence : class, IPersistenceModel
    {
        private string _memo;
        
        public string Memo
        {
            get => _memo;
            set
            {
                if(_memo == value) return;
                _memo = value;
                UpdateAndNotify();
            }
        }
        
        protected TransLike(
            TPersistence backingPersistenceModel,
            IRepository<TDomain, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            bool isInserted, 
            string memo) : base(backingPersistenceModel, isInserted, repository, rxSchedulerProvider)
        {
            _memo = memo ?? _memo;
        }

    }
}
