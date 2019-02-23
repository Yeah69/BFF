using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BFF.Persistence.Models
{
    public interface IPersistenceModel
    {
        Task<bool> InsertAsync();

        Task UpdateAsync();

        Task DeleteAsync();
    }
}
