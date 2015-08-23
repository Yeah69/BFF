using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Payee : DataModelBase
    {

        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public string Name { get; set; }

        #endregion


    }
}
