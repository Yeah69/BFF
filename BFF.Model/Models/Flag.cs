using System.Drawing;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }

        Task MergeToAsync(IFlag flag);
    }

    public abstract class Flag : CommonProperty, IFlag
    {
        private Color _color;

        public Flag(
            Color color, 
            string name) : base(name)
        {
            _color = color;
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value) return;
                _color = value;
                UpdateAndNotify();
            }
        }

        public abstract Task MergeToAsync(IFlag flag);
    }
}
