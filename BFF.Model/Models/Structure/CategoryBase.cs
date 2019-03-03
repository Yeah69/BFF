using BFF.Core.Helper;

namespace BFF.Model.Models.Structure
{
    public interface ICategoryBase : ICommonProperty
    {
    }

    public abstract class CategoryBase : CommonProperty, ICategoryBase
    {
        protected CategoryBase(
            IRxSchedulerProvider rxSchedulerProvider,
            string name) : base(rxSchedulerProvider, name)
        {
        }
    }
}
