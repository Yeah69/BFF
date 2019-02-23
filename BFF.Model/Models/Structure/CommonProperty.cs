using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface ICommonProperty : IDataModel
    {
        /// <summary>
        /// Name of the CommonProperty
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// CommonProperties are classes, whose instances are shared among other model classes
    /// </summary>
    internal abstract class CommonProperty<TDomain, TPersistence> 
        : DataModel<TDomain, TPersistence>, 
            ICommonProperty
        where TDomain : class, ICommonProperty
        where TPersistence : class, IPersistenceModel
    {
        private string _name;

        /// <summary>
        /// Name of the CommonProperty
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                UpdateAndNotify();
            }
        }

        /// <summary>
        /// Representing String
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }

        protected CommonProperty(
            TPersistence backingPersistenceModel,
            IRepository<TDomain, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            bool isInserted = false, 
            string name = null) : base(backingPersistenceModel, isInserted, repository, rxSchedulerProvider)
        {
            _name = name;
        }
    }
}
