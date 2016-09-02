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
            ConstrDbLock = true;

            Name = "All Accounts"; //todo Localize! Maybe then override the Name property

            ConstrDbLock = false;
        }

        protected override void InsertToDb()
        {
        }

        protected override void UpdateToDb()
        {
        }

        protected override void DeleteFromDb()
        {
        }

        public override bool ValidToInsert()
        {
            return false;
        }
    }
}
