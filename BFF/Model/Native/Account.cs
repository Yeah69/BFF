using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Account : DataModelBase
    {
        #region Non-Static
        
        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public string Name { get; set; }

        [DataField]
        public double RunninBalance { get; set; }

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables



        #endregion

        #region Static Methods



        #endregion

        #endregion
    }
}
