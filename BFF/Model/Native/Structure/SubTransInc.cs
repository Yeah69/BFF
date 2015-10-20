﻿using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    abstract class SubTransInc : DataModelBase
    {
        [Write(false)]
        public abstract TransactionIncome Parent { get; set; }

        // todo: Find a way for setter of ParentId
        public abstract long ParentId { get; }

        [Write(false)]
        public abstract Category Category { get; set; }

        public abstract long CategoryId { get; set; }

        public abstract string Memo { get; set; }
        
        public abstract double Sum { get; set; }
    }
}
