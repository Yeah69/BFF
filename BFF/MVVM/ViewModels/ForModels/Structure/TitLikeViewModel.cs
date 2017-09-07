using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ITitLikeViewModel : IDataModelViewModel
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
    public abstract class TitLikeViewModel : DataModelViewModel, ITitLikeViewModel
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
        /// Initializes a TitLikeViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="titLike">The model.</param>
        protected TitLikeViewModel(IBffOrm orm, ITitLike titLike) : base(orm, titLike)
        {
            Memo = titLike.ToReactivePropertyAsSynchronized(tl => tl.Memo);

            InitializeDeleteCommand();
        }

        /// <summary>
        /// Each TIT can be deleted from the GUI.
        /// </summary>
        /// <remarks>
        /// In order to subscribe a different callback on the DeleteCommand override <see cref="InitializeDeleteCommand" /> and do the subscription the.
        /// However, do not call the overridden function. It will be called in the constructor of <see cref="TitLikeViewModel"/>.
        /// </remarks>
        public ReactiveCommand DeleteCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Override this function for internal subscription to the DeleteCommand. This will be called once in the constructor of the <see cref="TitLikeViewModel"/>.
        /// Hence, by only subscribing in this overridden function a single subscription is guaranteed. 
        /// </summary>
        protected virtual void InitializeDeleteCommand()
        {
            DeleteCommand.Subscribe(_ => Delete());
        }
    }
}
