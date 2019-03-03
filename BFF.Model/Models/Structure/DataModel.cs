using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BFF.Core.Helper;

namespace BFF.Model.Models.Structure
{
    public interface IDataModel : IObservableObject
    {
        bool IsInserted { get; }

        Task InsertAsync();

        Task DeleteAsync();
    }

    public abstract class DataModel : ObservableObject, IDataModel
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;

        protected DataModel(
            IRxSchedulerProvider rxSchedulerProvider)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
        }

        public abstract bool IsInserted { get; }

        public abstract Task InsertAsync();

        protected abstract Task UpdateAsync();

        public abstract Task DeleteAsync();

        protected Task UpdateAndNotify([CallerMemberName] string propertyName = "") => 
            Task.Run(UpdateAsync)
                .ToObservable()
                .ObserveOn(_rxSchedulerProvider.UI)
                .Do(_ => OnPropertyChanged(propertyName))
                .ToTask();
    }
}
