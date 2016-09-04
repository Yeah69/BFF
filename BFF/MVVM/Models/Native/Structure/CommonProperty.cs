namespace BFF.MVVM.Models.Native.Structure
{
    public interface ICommonProperty : IDataModelBase
    {
        /// <summary>
        /// Name of the CommonProperty
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    /// CommonProperties are classes, whose instances are shared among other model classes
    /// </summary>
    public abstract class CommonProperty : DataModelBase, ICommonProperty
    {
        private string _name;

        /// <summary>
        /// Name of the CommonProperty
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value; 
                OnPropertyChanged();
            }
        }

        protected CommonProperty(string name = null)
        {
            _name = name;
        }
    }
}
