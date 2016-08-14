using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    /// <summary>
    /// Base class for ViewModels of the Models ParentTransaction and ParentIncome
    /// </summary>
    /// <typeparam name="T">Type of the SubElement. Can be a SubTransaction or a SubIncome.</typeparam>
    abstract class ParentTransIncViewModel<T> : TransIncBaseViewModel where T : ISubTransInc
    {
        /// <summary>
        /// Model of ParentTransaction or ParentIncome. Mostly they both act almost the same. Differences are handled in their concrete classes.
        /// </summary>
        protected IParentTransInc<T> ParentTransInc;

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => ParentTransInc.Id;

        /// <summary>
        /// The assigned Account, where this ParentTransaction/ParentIncome is registered.
        /// </summary>
        public override Account Account
        {
            get
            {
                return ParentTransInc.AccountId == -1 ? null : 
                    Orm?.CommonPropertyProvider?.GetAccount(ParentTransInc.AccountId);
            }
            set
            {
                Account temp = Account;
                ParentTransInc.AccountId = value?.Id ?? -1;
                Update();
                if (temp != null) Messenger.Default.Send(AccountMessage.Refresh, temp);
                if (value != null) Messenger.Default.Send(AccountMessage.Refresh, value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// This timestamp marks the time point, when the TIT happened.
        /// </summary>
        public override DateTime Date
        {
            get { return ParentTransInc.Date; }
            set
            {
                ParentTransInc.Date = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        public override Payee Payee
        {
            get
            {
                return ParentTransInc.PayeeId == -1 ? null :
                  Orm?.CommonPropertyProvider?.GetPayee(ParentTransInc.PayeeId);
            }
            set
            {
                ParentTransInc.PayeeId = value?.Id ?? -1;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public override string Memo
        {
            get { return ParentTransInc.Memo; }
            set
            {
                ParentTransInc.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money of the exchangement of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override long Sum
        {
            get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
            set { RefreshSum(); }
        }

        /// <summary>
        /// Refreshes the Balance of the associated account and the summary account and tells the GUI to refresh the sum of this ViewModel.
        /// </summary>
        internal void RefreshSum()
        {
            Messenger.Default.Send(AllAccountMessage.RefreshBalance);
            Messenger.Default.Send(AccountMessage.RefreshBalance, Account);
            OnPropertyChanged(nameof(Sum));
        }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public override bool Cleared
        {
            get { return ParentTransInc.Cleared; }
            set
            {
                ParentTransInc.Cleared = value;
                Update();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SubTransIncViewModel<T>> _subElements; 

        /// <summary>
        /// The SubElements of this ParentElement, which are inserted into the database already.
        /// </summary>
        public ObservableCollection<SubTransIncViewModel<T>> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    IEnumerable<T> subs = Orm?.GetSubTransInc<T>(ParentTransInc.Id) ?? new List<T>();
                    _subElements = new ObservableCollection<SubTransIncViewModel<T>>();
                    foreach(T sub in subs)
                    {
                        _subElements.Add(CreateNewSubViewModel(sub));
                    }
                }
                return _subElements;
            }
            set
            {
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The concrete Parent class should provide a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected abstract SubTransIncViewModel<T> CreateNewSubViewModel(T subElement);

        private readonly ObservableCollection<SubTransIncViewModel<T>> _newSubElements = new ObservableCollection<SubTransIncViewModel<T>>();

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        public ObservableCollection<SubTransIncViewModel<T>> NewSubElements
        {
            get { return _newSubElements; }
            set
            {
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a ParentTransIncViewModel.
        /// </summary>
        /// <param name="transInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        protected ParentTransIncViewModel(IParentTransInc<T> transInc, IBffOrm orm) : base(orm)
        {
            ParentTransInc = transInc;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        internal override bool ValidToInsert()
        {
            return Account != null && (Orm?.CommonPropertyProvider.Accounts.Contains(Account) ?? false) &&
                   Payee   != null &&  Orm .CommonPropertyProvider.Payees.Contains(Payee) && 
                   NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        /// <summary>
        /// Removes the given SubElement and refreshes the sum.
        /// </summary>
        /// <param name="toRemove"></param>
        public void RemoveSubElement(SubTransIncViewModel<T> toRemove)
        {
            if (SubElements.Contains(toRemove))
                SubElements.Remove(toRemove);
            RefreshSum();
        }

        /// <summary>
        /// Deletes the Model from the database and all ist SubElements, which are already in the database.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            foreach (SubTransIncViewModel<T> subTransaction in SubElements)
                subTransaction.Delete();
            SubElements.Clear();
            NewSubElements.Clear();
            Delete();
        });
        
        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        public ICommand NewSubElementCommand => new RelayCommand(obj => _newSubElements.Add(CreateNewSubViewModel(CreateNewSubElement())));

        /// <summary>
        /// The concrete Parent class should provide a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public abstract T CreateNewSubElement();
        
        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (SubTransIncViewModel<T> subTransaction in _newSubElements)
            {
                if (Id > 0L)
                    subTransaction.Insert();
                _subElements.Add(subTransaction);
            }
            _newSubElements.Clear();
            OnPropertyChanged(nameof(Sum));
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        public ICommand OpenParentTitView => new RelayCommand(param =>
                    Messenger.Default.Send(new ParentTitViewModel(ParentTransInc, "Yeah69", param as Account)));
    }
}
