namespace BFF.MVVM.Models.Native
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccount : Account
    {
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public SummaryAccount()
        {
            ConstrDbLock = true;

            Name = "All Accounts";

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
