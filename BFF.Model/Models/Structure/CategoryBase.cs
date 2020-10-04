namespace BFF.Model.Models.Structure
{
    public interface ICategoryBase : ICommonProperty
    {
    }

    public abstract class CategoryBase : CommonProperty, ICategoryBase
    {
        protected CategoryBase(string name) : base(name)
        {
        }
    }
}
