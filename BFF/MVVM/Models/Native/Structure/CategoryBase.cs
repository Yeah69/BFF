using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ICategoryBase : ICommonProperty
    {
    }
    
    public abstract class CategoryBase<T> : CommonProperty<T>, ICategoryBase where T : class, ICategoryBase
    {
        protected CategoryBase(IRepository<T> repository, long id, string name) : base(repository, id, name)
        {
        }
    }
}
