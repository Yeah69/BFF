using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface IParentTransInc : ITransIncBase {}

    public abstract class ParentTransInc<T> : TransIncBase<T>, IParentTransInc where T : class, IParentTransInc
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected ParentTransInc(IRepository<T> repository, 
                                 DateTime date, 
                                 IAccount account = null, 
                                 IPayee payee = null, 
                                 string memo = null, 
                                 bool? cleared = null) 
            : base(repository, date, account, payee, memo, cleared) {}

        /// <summary>
        /// Safe ORM-constructor
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="accountId">Id of Account</param>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="payeeId">Id of Payee</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected ParentTransInc(IRepository<T> repository, 
                                 long id, 
                                 long accountId, 
                                 DateTime date, 
                                 long payeeId, 
                                 string memo, 
                                 bool cleared) 
            : base(repository, id, accountId, date, payeeId, memo, cleared) {}
    }
}
