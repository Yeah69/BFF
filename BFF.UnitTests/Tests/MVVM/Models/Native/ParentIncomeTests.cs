using System;
using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class ParentIncomeTests : ParentTransIncTests<ParentIncome>
    {
        protected override ParentIncome DataModelBaseFactory => new ParentIncome(IdInitialValue, 1, new DateTime(1969, 6, 9), 1, "Yeah, Party!", false);
        protected override long IdInitialValue => 1;
        protected override long IdDifferentValue => 2;

        protected override ParentIncome TitLikeFactory => new ParentIncome(1, 1, new DateTime(1969, 6, 9), 1, MemoInitialValue, false);
        protected override string MemoInitialValue => "Yeah, Party!";
        protected override string MemoDifferentValue => "Party, Yeah!";
        
        protected override ParentIncome TitBaseFactory => new ParentIncome(1, 1, DateInitialValue, 1, "Yeah, Party!", ClearedInitialValue);
        protected override DateTime DateInitialValue => new DateTime(1969, 6, 9);
        protected override DateTime DateDifferentValue => new DateTime(1969, 9, 6);
        protected override bool ClearedInitialValue => false;
        protected override bool ClearedDifferentValue => true;

        protected override ParentIncome TransIncBaseFactory => new ParentIncome(1, AccountIdInitialValue, new DateTime(1969, 6, 9), PayeeIdInitialValue, "Yeah, Party!", false);
        protected override long AccountIdInitialValue => 3;
        protected override long AccountIdDifferentValue => 4;
        protected override long PayeeIdInitialValue => 5;
        protected override long PayeeIdDifferentValue => 6;
    }
}