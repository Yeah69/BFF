namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// CommonProperties are classes, whose instances are shared among other model classes
    /// </summary>
    public abstract class CommonProperty : DataModelBase
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
                _name = value; 
                if(Id != -1) Update();
                OnPropertyChanged();
            }
        }

        protected CommonProperty(string name = null)
        {
            ConstrDbLock = true;

            _name = name;

            ConstrDbLock = false;
        }

        public override bool ValidToInsert()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
