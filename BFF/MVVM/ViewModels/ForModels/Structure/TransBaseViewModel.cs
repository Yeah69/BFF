using System;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransBaseViewModel : ITransLikeViewModel
    {
        IObservableReadOnlyList<IFlagViewModel> AllFlags { get; }

        IReactiveProperty<IFlagViewModel> Flag { get; }

        ReactiveCommand RemoveFlag { get; }

        IReactiveProperty<string> CheckNumber { get; }

        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        IReactiveProperty<DateTime> Date { get; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        IReactiveProperty<bool> Cleared { get; }
        INewFlagViewModel NewFlagViewModel { get; }
    }

    /// <summary>
    /// Base class for all ViewModels of Models of TITs excluding the SubElements.
    /// From this point in the documentation of the ViewModel hierarchy TIT is referring to all TIT-like Elements except SubElements.
    /// </summary>
    public abstract class TransBaseViewModel : TransLikeViewModel, ITransBaseViewModel, IHaveFlagViewModel
    {
        private readonly IFlagViewModelService _flagViewModelService;
        public IObservableReadOnlyList<IFlagViewModel> AllFlags => _flagViewModelService.All;

        public IReactiveProperty<IFlagViewModel> Flag { get; }
        public ReactiveCommand RemoveFlag { get; }

        public IReactiveProperty<string> CheckNumber { get; }

        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public IReactiveProperty<DateTime> Date { get; }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public virtual IReactiveProperty<bool> Cleared { get; }

        public INewFlagViewModel NewFlagViewModel { get; }

        protected abstract void NotifyRelevantAccountsToRefreshTits();

        protected abstract void NotifyRelevantAccountsToRefreshBalance();

        protected TransBaseViewModel(
            ITransBase transBase,
            Func<IHaveFlagViewModel, INewFlagViewModel> newFlagViewModelFactory,
            ILastSetDate lastSetDate,
            IFlagViewModelService flagViewModelService)
            : base(transBase)
        {
            _flagViewModelService = flagViewModelService;

            NewFlagViewModel = newFlagViewModelFactory(this);

            Flag = transBase.ToReactivePropertyAsSynchronized(
                tb => tb.Flag, 
                flagViewModelService.GetViewModel,
                flagViewModelService.GetModel,
                ReactivePropertyMode.DistinctUntilChanged);

            RemoveFlag = new ReactiveCommand().AddTo(CompositeDisposable);

            RemoveFlag.Subscribe(() => Flag.Value = null);

            CheckNumber = transBase.ToReactivePropertyAsSynchronized(tb => tb.CheckNumber, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Date = transBase.ToReactivePropertyAsSynchronized(tb => tb.Date, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Date.Subscribe(dt => lastSetDate.Date = dt).AddTo(CompositeDisposable);

            Date.Where(_ => transBase.Id != -1).Subscribe(_ => NotifyRelevantAccountsToRefreshTits()).AddTo(CompositeDisposable);

            Cleared = transBase.ToReactivePropertyAsSynchronized(tb => tb.Cleared, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }
    }
}
