using BFF.DB;

namespace BFF.MVVM.Models.Native
{
    public interface ISummaryAccount : IAccount {}

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccount : Account, ISummaryAccount
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount()
        {
            Name = "All Accounts"; //todo Localize! Maybe then override the Name property
        }

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm) {}

        public override void Update(IBffOrm orm) {}

        public override void Delete(IBffOrm orm){}

        #endregion
    }
}
