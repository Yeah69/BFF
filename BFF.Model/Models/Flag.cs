using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories.ModelRepositories;

namespace BFF.Model.Models
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }

        Task MergeToAsync(IFlag flag);
    }

    internal class Flag : CommonProperty<IFlag>, IFlag
    {
        public static IFlag Default = new Flag(null, null, Color.BlueViolet, -2, "Default");

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

        public Task MergeToAsync(IFlag flag)
        {
            return _repository.MergeAsync(@from: this, to: flag);
        }
    }
}
