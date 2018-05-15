using System;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.Helper.Extensions;
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
            INewFlagViewModel newFlagViewModel,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider schedulerProvider,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner)
            : base(transBase, schedulerProvider, owner)
        {
            _flagViewModelService = flagViewModelService;

            NewFlagViewModel = newFlagViewModel;

            Flag = transBase.ToReactivePropertyAsSynchronized(
                nameof(transBase.Flag),
                () => transBase.Flag,
                f => transBase.Flag = f,
                flagViewModelService.GetViewModel,
                flagViewModelService.GetModel,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged);

            RemoveFlag = new ReactiveCommand().AddTo(CompositeDisposable);

            RemoveFlag.Subscribe(() => Flag.Value = null);

            CheckNumber = transBase.ToReactivePropertyAsSynchronized(
                nameof(transBase.CheckNumber),
                () => transBase.CheckNumber,
                f => transBase.CheckNumber = f,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Date = transBase.ToReactivePropertyAsSynchronized(
                nameof(transBase.Date),
                () => transBase.Date,
                d => transBase.Date = d,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            Date.Subscribe(dt => lastSetDate.Date = dt).AddTo(CompositeDisposable);

            Date.Where(_ => transBase.Id != -1).Subscribe(_ => NotifyRelevantAccountsToRefreshTits()).AddTo(CompositeDisposable);

            Cleared = transBase.ToReactivePropertyAsSynchronized(
                nameof(transBase.Cleared),
                () => transBase.Cleared,
                c => transBase.Cleared = c,
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }
    }
}
