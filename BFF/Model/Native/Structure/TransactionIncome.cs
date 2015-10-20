using System;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    abstract class TransactionIncome : DataModelBase
    {
        [Write(false)]
        public abstract Account Account { get; set; }

        public abstract long AccountId { get; set; }

        public abstract DateTime Date { get; set; }

        [Write(false)]
        public abstract Payee Payee { get; set; }

        public abstract long PayeeId { get; set; }

        [Write(false)]
        public abstract Category Category { get; set; }

        public abstract long? CategoryId { get; set; }

        public abstract string Memo { get; set; }

        [Write(false)]
        public double ShowSum { get; set; } 

        public abstract double? Sum { get; set; }

        public abstract bool Cleared { get; set; }
    }
}
