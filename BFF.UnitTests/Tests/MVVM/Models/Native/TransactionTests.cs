using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class TransactionTests : TransIncTests<Transaction>
    {
        //todo: Should the constructor (also default values) be tested?
        protected override Transaction DataModelBaseFactory => new Transaction(IdInitialValue, 1, new DateTime(1969, 6, 9), 1, 1, "Yeah, Party", 69, false);

        protected override long IdInitialValue => 69;

        protected override long IdDifferentValue => 23;

        protected override Transaction TitLikeFactory => new Transaction(new DateTime(1969, 6, 9), memo: MemoInitialValue);

        protected override string MemoInitialValue => "Yeah, Party";

        protected override string MemoDifferentValue => "Party, Yeah";

        protected override Transaction TitBaseFactory => new Transaction(DateInitialValue, cleared: ClearedInitialValue);

        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);

        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);

        protected override bool ClearedInitialValue => false;

        protected override bool ClearedDifferentValue => true;

        protected override Transaction TransIncBaseFactory => new Transaction(1, AccountIdInitialValue, new DateTime(1969, 6, 9), PayeeIdInitialValue, 1, "Yeah, Party!", 69, true);

        protected override long AccountIdInitialValue => 69;

        protected override long AccountIdDifferentValue => 23;

        protected override long PayeeIdInitialValue => 69;

        protected override long PayeeIdDifferentValue => 23;

        protected override Transaction TransIncFactory => new Transaction(1, 1, new DateTime(1969, 6, 9), 1, CategoryIdDifferentValue, "Yeah, Party!", SumInitialValue, true);

        protected override long CategoryIdInitialValue => 69;

        protected override long CategoryIdDifferentValue => 23;

        protected override long SumInitialValue => 69;

        protected override long SumDifferentValue => 23;
    }
}