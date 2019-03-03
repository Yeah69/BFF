using BFF.Core.Helper;

namespace BFF.Model.Models.Structure
{
    public interface ITransLike : IDataModel
    {
        string Memo { get; set; }
    }

    public abstract class TransLike : DataModel, ITransLike
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
            IRxSchedulerProvider rxSchedulerProvider,
            string memo) : base(rxSchedulerProvider)
        {
            _memo = memo ?? _memo;
        }

    }
}
