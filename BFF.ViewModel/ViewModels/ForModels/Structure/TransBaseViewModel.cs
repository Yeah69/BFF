using System;
using System.Reactive.Linq;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using MuVaViMo;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface ITransBaseViewModel : ITransLikeViewModel
    {
        IObservableReadOnlyList<IFlagViewModel> AllFlags { get; }

        IFlagViewModel Flag { get; set; }

        IRxRelayCommand RemoveFlag { get; }

        string CheckNumber { get; set; }

        /// <summary>
        /// This timestamp marks the time point, when the Trans happened.
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark Trans', which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Trans later.
        /// </summary>
        bool Cleared { get; set; }
        INewFlagViewModel NewFlagViewModel { get; }
    }

    /// <summary>
    /// Base class for all ViewModels of Models of Trans' excluding the SubTransactions.
    /// From this point in the documentation of the ViewModel hierarchy Trans is referring to all Trans-like Elements except SubTransactions.
    /// </summary>
    public abstract class TransBaseViewModel : TransLikeViewModel, ITransBaseViewModel, IHaveFlagViewModel
    {
        private readonly ITransBase _transBase;
        private readonly IFlagViewModelService _flagViewModelService;
        private IFlagViewModel _flag;
        public IObservableReadOnlyList<IFlagViewModel> AllFlags => _flagViewModelService.All;

        public IFlagViewModel Flag
        {
            get => _flag;
            set => _transBase.Flag = _flagViewModelService.GetModel(value);
        }

        public IRxRelayCommand RemoveFlag { get; }

        public string CheckNumber
        {
            get => _transBase.CheckNumber;
            set => _transBase.CheckNumber = value;
        }

        /// <summary>
        /// This timestamp marks the time point, when the Trans happened.
        /// </summary>
        public DateTime Date
        {
            get => _transBase.Date;
            set => _transBase.Date = value;
        }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark Trans-elements, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Trans-element later.
        /// </summary>
        public virtual bool Cleared
        {
            get => _transBase.Cleared;
            set => _transBase.Cleared = value;
        }

        public INewFlagViewModel NewFlagViewModel { get; }

        protected abstract void NotifyRelevantAccountsToRefreshTrans();

        protected abstract void NotifyRelevantAccountsToRefreshBalance();

        protected TransBaseViewModel(
            ITransBase transBase,
            INewFlagViewModel newFlagViewModel,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider rxSchedulerProvider,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner)
            : base(transBase, rxSchedulerProvider, owner)
        {
            _transBase = transBase;
            _flagViewModelService = flagViewModelService;

            NewFlagViewModel = newFlagViewModel;

            _flag = _flagViewModelService.GetViewModel(transBase.Flag);
            transBase
                .ObservePropertyChanges(nameof(transBase.Flag))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _flag = _flagViewModelService.GetViewModel(transBase.Flag);
                    OnPropertyChanged(nameof(Flag));
                })
                .AddTo(CompositeDisposable);

            RemoveFlag = new RxRelayCommand(() => Flag = null).AddTo(CompositeDisposable);

            transBase
                .ObservePropertyChanges(nameof(transBase.CheckNumber))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(CheckNumber)))
                .AddTo(CompositeDisposable);

            transBase
                .ObservePropertyChanges(nameof(transBase.Date))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    lastSetDate.Date = transBase.Date;
                    OnPropertyChanged(nameof(Date));
                })
                .AddTo(CompositeDisposable);

            transBase
                .ObservePropertyChanges(nameof(transBase.Date))
                .Where(_ => transBase.Id != -1)
                .Subscribe(_ => NotifyRelevantAccountsToRefreshTrans())
                .AddTo(CompositeDisposable);
            
            transBase
                .ObservePropertyChanges(nameof(transBase.Cleared))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    OnPropertyChanged(nameof(Cleared));
                    NotifyRelevantAccountsToRefreshBalance();
                })
                .AddTo(CompositeDisposable);
        }
    }
}
