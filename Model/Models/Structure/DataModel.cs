using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MrMeeseeks.Windows;

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
        public abstract bool IsInserted { get; }

        public abstract Task InsertAsync();

        protected abstract Task UpdateAsync();

        public abstract Task DeleteAsync();

        protected Task UpdateAndNotify([CallerMemberName] string propertyName = "") => 
            Task.Run(UpdateAsync)
                .ToObservable()
                .Do(_ => OnPropertyChanged(propertyName))
                .ToTask();
    }
}
