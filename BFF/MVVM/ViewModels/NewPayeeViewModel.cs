using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    public interface INewPayeeViewModel
    {
        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        IReactiveProperty<string> PayeeText { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        ReactiveCommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        IObservableReadOnlyList<IPayeeViewModel> AllPayees { get; }
    }

    public class NewPayeeViewModel : INewPayeeViewModel
    {
        private readonly PayeeViewModelService _payeeViewModelService;

        public NewPayeeViewModel(
            IHavePayeeViewModel payeeOwner,
            IPayeeRepository payeeRepository,
            PayeeViewModelService payeeViewModelService)
        {
            _payeeViewModelService = payeeViewModelService;
            
            var observeCollection = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(AllPayees, "CollectionChanged");

            AddPayeeCommand = PayeeText.CombineLatest(observeCollection, (text, _) =>
                !string.IsNullOrEmpty(PayeeText?.Value?.Trim()) &&
                AllPayees.All(payee => payee.Name.Value != PayeeText?.Value.Trim())).ToReactiveCommand();

            AddPayeeCommand.Subscribe(() =>
            {
                IPayee newPayee = payeeRepository.Create();
                newPayee.Name = PayeeText.Value.Trim();
                newPayee.Insert();
                payeeOwner.Payee.Value = _payeeViewModelService.GetViewModel(newPayee);
            });
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public IReactiveProperty<string> PayeeText { get; } = new ReactiveProperty<string>();

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ReactiveCommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IPayeeViewModel> AllPayees => _payeeViewModelService.All;
    }
}
