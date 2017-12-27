using System.Windows.Media;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }
    }

    public class Flag : CommonProperty<IFlag>, IFlag
    {
        public static IFlag Default = new Flag(null, -2, "Default", Colors.Transparent);
        
        private Color _color;

        public Flag(IRepository<IFlag> repository, long id, string name, Color color) : base(repository, id, name)
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
                Update();
                OnPropertyChanged();
            }
        }
    }
}
