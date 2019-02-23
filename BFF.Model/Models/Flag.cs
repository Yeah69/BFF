using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface IFlag : ICommonProperty
    {
        Color Color { get; set; }

        Task MergeToAsync(IFlag flag);
    }

    internal class Flag<TPersistence> : CommonProperty<IFlag, TPersistence>, IFlag
        where TPersistence : class, IPersistenceModel
    {
        private readonly IMergingRepository<IFlag> _mergingRepository;
        private Color _color;

        public Flag(
            TPersistence backingPersistenceModel,
            IMergingRepository<IFlag> mergingRepository,
            IRepository<IFlag, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider, 
            Color color, 
            bool isInserted = false, 
            string name = "") : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, name)
        {
            _mergingRepository = mergingRepository;
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
            return _mergingRepository.MergeAsync(@from: this, to: flag);
        }
    }
}
