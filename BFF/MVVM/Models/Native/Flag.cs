using System.Threading.Tasks;
using System.Windows.Media;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }

        Task MergeTo(IFlag flag);
    }

    public class Flag : CommonProperty<IFlag>, IFlag
    {
        public static IFlag Default = new Flag(null, null, Colors.BlueViolet, -2, "Default");

        private readonly IFlagRepository _repository;
        private Color _color;

        public Flag(
            IFlagRepository repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            Color color, 
            long id = -1, 
            string name = "") : base(repository, rxSchedulerProvider, id, name)
        {
            _repository = repository;
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

        public Task MergeTo(IFlag flag)
        {
            return _repository.MergeAsync(from: this, to: flag);
        }
    }
}
