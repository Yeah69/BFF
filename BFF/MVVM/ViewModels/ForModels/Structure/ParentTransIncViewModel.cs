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
        private readonly IParentTransInc _parentTransInc;

        /// <summary>
        /// The amount of money of the exchange of the ParentTransaction or ParentIncome.
        /// A ParentElement's Sum is defined by the Sum of all Sum's of its SubElements.
        /// </summary>
        public override long Sum
        {
            get { return SubElements.Sum(subElement => subElement.Sum); } //todo: Write an SQL query for that
            set => RefreshSum();
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
                    IEnumerable<ISubTransInc> subs = GetSubTransInc() ?? new List<ISubTransInc>();
                    _subElements = new ObservableCollection<ISubTransIncViewModel>();
                    foreach(ISubTransInc sub in subs)
                    {
                        _subElements.Add(CreateNewSubViewModel(sub));
                    }
                }
                return _subElements;
            }
            set => OnPropertyChanged();
        }

        /// <summary>
        /// The concrete Parent class should provide a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected abstract ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement);
        protected abstract IEnumerable<ISubTransInc> GetSubTransInc();

        private readonly ObservableCollection<ISubTransIncViewModel> _newSubElements = new ObservableCollection<ISubTransIncViewModel>();

        /// <summary>
        /// The SubElements of this ParentElement, which are not inserted into the database yet.
        /// These SubElements are in the process of being created and inserted to the database.
        /// </summary>
        public ObservableCollection<ISubTransIncViewModel> NewSubElements
        {
            get => _newSubElements;
            set => OnPropertyChanged();
        }

        /// <summary>
        /// Initializes a ParentTransIncViewModel.
        /// </summary>
        /// <param name="transInc">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        protected ParentTransIncViewModel(IParentTransInc transInc, IBffOrm orm) : base(orm, transInc)
        {
            _parentTransInc = transInc;
            _parentTransInc.PropertyChanged += (sender, args) =>
            {
                switch(args.PropertyName)
                {
                    case nameof(_parentTransInc.Id):
                        foreach(ISubTransIncViewModel subTransIncViewModel in SubElements)
                        {
                            subTransIncViewModel.ParentId = _parentTransInc.Id;
                        }
                        foreach(ISubTransIncViewModel subTransIncViewModel in NewSubElements)
                        {
                            subTransIncViewModel.ParentId = _parentTransInc.Id;
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
            return Account != null  && Payee != null && NewSubElements.All(subElement => subElement.ValidToInsert());
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
            _parentTransInc.Insert();
        }
    }
}
