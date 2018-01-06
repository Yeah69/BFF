using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ICategoryBase : ICommonProperty
    {
    }

    /// <summary>
    /// This CommonProperty is used to categorize Tits
    /// </summary>
    public abstract class CategoryBase<T> : CommonProperty<T>, ICategoryBase where T : class, ICategoryBase
    {
        /// <summary>
        /// Initializes the Object
        /// </summary>
        /// <param name="name">Name of the Category</param>
        protected CategoryBase(IRepository<T> repository, long id, string name) : base(repository, id, name)
        {
        }
    }
}
