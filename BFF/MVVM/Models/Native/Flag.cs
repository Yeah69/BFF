using System.Windows.Media;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }
    }

    public class Flag : CommonProperty<IFlag>, IFlag
    {
        public static IFlag Default = new Flag(null, null, Colors.BlueViolet, -2, "Default");
        
        private Color _color;

        public Flag(
            IRepository<IFlag> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            Color color, 
            long id = -1, 
            string name = "") : base(repository, rxSchedulerProvider, id, name)
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
    }
}
