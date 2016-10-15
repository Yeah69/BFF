﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IParentTransIncViewModel : ITransIncBaseViewModel {
        /// <summary>
        /// Removes the given SubElement and refreshes the sum.
        /// </summary>
        /// <param name="toRemove"></param>
        void RemoveSubElement(ISubTransIncViewModel toRemove);

        /// <summary>
        /// Creates a new SubElement for this ParentElement.
        /// </summary>
        ICommand NewSubElementCommand { get; }

        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        ICommand ApplyCommand { get; }

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        ICommand OpenParentTitView { get; }

        /// <summary>
        /// The SubElements of this ParentElement, which are inserted into the database already.
        /// </summary>
        ObservableCollection<ISubTransIncViewModel> SubElements { get; set; }

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        ObservableCollection<ISubTransIncViewModel> NewSubElements { get; set; }

        /// <summary>
        /// Refreshes the Balance of the associated account and the summary account and tells the GUI to refresh the sum of this ViewModel.
        /// </summary>
        void RefreshSum();
    }

    /// <summary>
    /// Base class for ViewModels of the Models ParentTransaction and ParentIncome
    /// </summary>
    public abstract class ParentTransIncViewModel : TransIncBaseViewModel, IParentTransIncViewModel
    {
        /// <summary>
        /// Model of ParentTransaction or ParentIncome. Mostly they both act almost the same. Differences are handled in their concrete classes.
        /// </summary>
        protected IParentTransInc ParentTransInc;

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => ParentTransInc.Id;

        /// <summary>
        /// The assigned Account, where this ParentTransaction/ParentIncome is registered.
        /// </summary>
        public override IAccountViewModel Account
        {
            get
            {
                return ParentTransInc.AccountId == -1 ? null : 
                    Orm?.CommonPropertyProvider?.GetAccountViewModel(ParentTransInc.AccountId);
            }
            set
            {
                IAccountViewModel temp = Account;
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
        public override IPayeeViewModel Payee
        {
            get
            {
                return ParentTransInc.PayeeId == -1 ? null :
                  Orm?.CommonPropertyProvider?.GetPayeeViewModel(ParentTransInc.PayeeId);
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
        public void RefreshSum()
        {
            if(Id > 0)
            {
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
                Messenger.Default.Send(AccountMessage.RefreshBalance, Account);
            }
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

        private ObservableCollection<ISubTransIncViewModel> _subElements; 

        /// <summary>
        /// The SubElements of this ParentElement, which are inserted into the database already.
        /// </summary>
        public ObservableCollection<ISubTransIncViewModel> SubElements
        {
            get
            {
                if (_subElements == null)
                {
                    IEnumerable<ISubTransInc> subs = ParentTransInc.GetSubTransInc(Orm) ?? new List<ISubTransInc>();
                    _subElements = new ObservableCollection<ISubTransIncViewModel>();
                    foreach(ISubTransInc sub in subs)
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
        protected abstract ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement);

        private readonly ObservableCollection<ISubTransIncViewModel> _newSubElements = new ObservableCollection<ISubTransIncViewModel>();

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        public ObservableCollection<ISubTransIncViewModel> NewSubElements
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
        protected ParentTransIncViewModel(IParentTransInc transInc, IBffOrm orm) : base(orm)
        {
            ParentTransInc = transInc;
            ParentTransInc.PropertyChanged += (sender, args) =>
            {
                switch(args.PropertyName)
                {
                    case nameof(ParentTransInc.Id):
                        foreach(ISubTransIncViewModel subTransIncViewModel in SubElements)
                        {
                            subTransIncViewModel.ParentId = ParentTransInc.Id;
                        }
                        foreach(ISubTransIncViewModel subTransIncViewModel in NewSubElements)
                        {
                            subTransIncViewModel.ParentId = ParentTransInc.Id;
                        }
                        break;
                }
            };
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Account != null && (Orm?.CommonPropertyProvider.AllAccountViewModels.Contains(Account) ?? false) &&
                   Payee   != null &&  Orm .CommonPropertyProvider.AllPayeeViewModels.Contains(Payee) && 
                   NewSubElements.All(subElement => subElement.ValidToInsert());
        }

        /// <summary>
        /// Removes the given SubElement and refreshes the sum.
        /// </summary>
        /// <param name="toRemove"></param>
        public void RemoveSubElement(ISubTransIncViewModel toRemove)
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
            foreach (ISubTransIncViewModel subTransaction in SubElements)
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
        public abstract ISubTransInc CreateNewSubElement();
        
        /// <summary>
        /// All new SubElement, which are not inserted into the database yet, will be flushed to the database with this command.
        /// </summary>
        public ICommand ApplyCommand => new RelayCommand(obj =>
        {
            foreach (ISubTransIncViewModel subTransaction in _newSubElements)
            {
                if (Id > 0L)
                    subTransaction.Insert();
                _subElements.Add(subTransaction);
            }
            _newSubElements.Clear();
            OnPropertyChanged(nameof(Sum));
            Messenger.Default.Send(SummaryAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        });

        /// <summary>
        /// Opens the Parent master page for this ParentElement.
        /// </summary>
        public ICommand OpenParentTitView => new RelayCommand(param =>
                    Messenger.Default.Send(new ParentTitViewModel(this, "Yeah69", param as IAccount)));

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            ParentTransInc.Insert(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            ParentTransInc.Update(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            ParentTransInc.Delete(Orm);
        }
    }
}