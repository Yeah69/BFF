using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class IncomeTests : TransIncTests<Income>
    {
        protected override Income DataModelBaseFactory => new Income(IdInitialValue, 1, new DateTime(1969, 6, 9), 1, 1, "Yeah, Party", 69, false);

        protected override long IdInitialValue => 69;

        protected override long IdDifferentValue => 23;

        protected override Income TitLikeFactory => new Income(new DateTime(1969, 6, 9), memo: MemoInitialValue);

        protected override string MemoInitialValue => "Yeah, Party";

        protected override string MemoDifferentValue => "Party, Yeah";

        protected override Income TitBaseFactory => new Income(DateInitialValue, cleared: ClearedInitialValue);

        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);

        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);

        protected override bool ClearedInitialValue => false;

        protected override bool ClearedDifferentValue => true;

        protected override Income TransIncBaseFactory => new Income(1, AccountIdInitialValue, new DateTime(1969, 6, 9), PayeeIdInitialValue, 1, "Yeah, Party!", 69, true);

        protected override long AccountIdInitialValue => 69;

        protected override long AccountIdDifferentValue => 23;

        protected override long PayeeIdInitialValue => 69;

        protected override long PayeeIdDifferentValue => 23;

        protected override Income TransIncFactory => new Income(1, 1, new DateTime(1969, 6, 9), 1, CategoryIdDifferentValue, "Yeah, Party!", SumInitialValue, true);

        protected override long CategoryIdInitialValue => 69;

        protected override long CategoryIdDifferentValue => 23;

        protected override long SumInitialValue => 69;

        protected override long SumDifferentValue => 23;
    }
}