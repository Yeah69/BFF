using BFF.Core;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransLike : IDataModel
    {
        string Memo { get; set; }
    }
    
    public abstract class TransLike<T> : DataModel<T>, ITransLike where T : class, ITransLike
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
            IRepository<T> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            long id, 
            string memo) : base(repository, rxSchedulerProvider, id)
        {
            _memo = memo ?? _memo;
        }

    }
}
