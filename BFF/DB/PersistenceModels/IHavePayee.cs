using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFF.DB.PersistenceModels
{
    public interface IHavePayee
    {
        long PayeeId { get; set; }
    }
}
