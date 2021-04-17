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
    public abstract class CommonProperty : DataModel, ICommonProperty
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
            string name)
        {
            _name = name;
        }
    }
}
