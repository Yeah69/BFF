using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITransLikeViewModel : IDataModelViewModel
    {
        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        IReactiveProperty<string> Memo { get; }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        ReactiveCommand DeleteCommand { get; }
    }

    /// <summary>
    /// Base class for all ViewModel classes of Models related to TITs (Transaction, Income, Transfer).
    /// This includes also the Parent and SubElement models.
    /// </summary>
    public abstract class TransLikeViewModel : DataModelViewModel, ITransLikeViewModel
    {

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public virtual IReactiveProperty<string> Memo { get; }

        /// <summary>
        /// The amount of money of the exchange of the TIT.
        /// </summary>
        public abstract IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a TransLikeViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="transLike">The model.</param>
        protected TransLikeViewModel(IBffOrm orm, ITransLike transLike) : base(orm, transLike)
        {
            Memo = transLike.ToReactivePropertyAsSynchronized(tl => tl.Memo, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            InitializeDeleteCommand();
        }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        /// <remarks>
        /// In order to subscribe a different callback on the DeleteCommand override <see cref="InitializeDeleteCommand" /> and do the subscription the.
        /// However, do not call the overridden function. It will be called in the constructor of <see cref="TransLikeViewModel"/>.
        /// </remarks>
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Override this function for internal subscription to the DeleteCommand. This will be called once in the constructor of the <see cref="TransLikeViewModel"/>.
        /// Hence, by only subscribing in this overridden function a single subscription is guaranteed. 
        /// </summary>
        protected virtual void InitializeDeleteCommand()
        {
            DeleteCommand.Subscribe(_ => Delete());
        }
    }
}
