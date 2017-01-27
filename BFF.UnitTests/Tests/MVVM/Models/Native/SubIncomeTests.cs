using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class SubIncomeTests : SubTransIncTests<SubIncome>
    {
        protected override SubIncome DataModelBaseFactory => new SubIncome(IdInitialValue, 1, 1, "Yeah, Party!", 69);

        protected override long IdInitialValue => 69;

        protected override long IdDifferentValue => 23;

        protected override SubIncome TitLikeFactory => new SubIncome(1, 1, 1, MemoInitialValue, 69);

        protected override string MemoInitialValue => "Yeah, Party!";

        protected override string MemoDifferentValue => "Party, Yeah!";

        protected override SubIncome SubTransIncFactory => new SubIncome(1, ParentIdInitialValue, CategoryIdInitialValue, "Yeah, Party!", SumInitialValue);

        protected override long ParentIdInitialValue => 1;

        protected override long ParentIdDifferentValue => 2;

        protected override long CategoryIdInitialValue => 3;

        protected override long CategoryIdDifferentValue => 4;

        protected override long SumInitialValue => 5;

        protected override long SumDifferentValue => 6;
    }
}