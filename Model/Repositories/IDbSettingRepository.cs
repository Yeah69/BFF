using System.Threading.Tasks;
using BFF.Model.Models;

namespace BFF.Model.Repositories
{
    public interface IDbSettingRepository
    {
        Task<IDbSetting> GetSetting();
    }
}