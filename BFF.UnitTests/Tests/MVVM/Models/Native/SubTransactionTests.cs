using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class SubTransactionTests : SubTransIncTests<SubTransaction>
    {
        protected override SubTransaction DataModelBaseFactory => new SubTransaction(IdInitialValue, 1, 1, "Yeah, Party!", 69);

        protected override long IdInitialValue => 69;

        protected override long IdDifferentValue => 23;

        protected override SubTransaction TitLikeFactory => new SubTransaction(1, 1, 1, MemoInitialValue, 69);

        protected override string MemoInitialValue => "Yeah, Party!";

        protected override string MemoDifferentValue => "Party, Yeah!";

        protected override SubTransaction SubTransIncFactory => new SubTransaction(1, ParentIdInitialValue, CategoryIdInitialValue, "Yeah, Party!", SumInitialValue);

        protected override long ParentIdInitialValue => 1;

        protected override long ParentIdDifferentValue => 2;

        protected override long CategoryIdInitialValue => 3;

        protected override long CategoryIdDifferentValue => 4;

        protected override long SumInitialValue => 5;

        protected override long SumDifferentValue => 6;
    }
}