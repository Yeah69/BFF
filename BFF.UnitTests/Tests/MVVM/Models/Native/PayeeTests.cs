using BFF.MVVM.Models.Native;
using BFF.Tests.Tests.MVVM.Models.Native.Structure;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public class PayeeTests : CommonPropertyTests<Payee>
    {
        protected override Payee DataModelBaseFactory => new Payee(IdInitialValue, "Mr. Unknown");
        protected override long IdInitialValue => 69;
        protected override long IdDifferentValue => 23;

        protected override Payee CommonPropertyFactory => new Payee(1, NameInitialValue);
        protected override string NameInitialValue => "Person A";
        protected override string NameDifferentValue => "Person B";
        protected override string ToStringExpectedValue => "Person A";
    }
}
