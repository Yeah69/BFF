using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

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

    public class NewPayeeViewModel : INewPayeeViewModel, IDisposable
    {
        private readonly IPayeeViewModelService _payeeViewModelService;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public NewPayeeViewModel(
            IHavePayeeViewModel payeeOwner,
            IPayeeRepository payeeRepository,
            Func<IPayee> payeeFactory,
            IPayeeViewModelService payeeViewModelService)
        {
            string ValidatePayeeName(string text)
            {
                return !string.IsNullOrWhiteSpace(text) &&
                    AllPayees.All(payee => payee.Name.Value != text.Trim()) 
                    ? null 
                    : "ErrorMessageWrongPayeeName".Localize();
            }

            _payeeViewModelService = payeeViewModelService;

            PayeeText = new ReactiveProperty<string>().SetValidateNotifyError(
                text => ValidatePayeeName(text)).AddTo(_compositeDisposable);

            AddPayeeCommand = new ReactiveCommand().AddTo(_compositeDisposable);

            AddPayeeCommand.Where(
                _ =>
                {
                    (PayeeText as ReactiveProperty<string>)?.ForceValidate();
                    return !PayeeText.HasErrors;
                })
                .Subscribe(_ =>
                {
                    IPayee newPayee = payeeFactory();
                    newPayee.Name = PayeeText.Value.Trim();
                    newPayee.Insert();
                    payeeOwner.Payee.Value = _payeeViewModelService.GetViewModel(newPayee);
                }).AddTo(_compositeDisposable);
        }

        /// <summary>
        /// User input of the to be searched or to be created Payee.
        /// </summary>
        public IReactiveProperty<string> PayeeText { get; }

        /// <summary>
        /// Creates a new Payee.
        /// </summary>
        public ReactiveCommand AddPayeeCommand { get; }

        /// <summary>
        /// All currently available Payees.
        /// </summary>
        public IObservableReadOnlyList<IPayeeViewModel> AllPayees => _payeeViewModelService.All;

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
        }
    }
}
