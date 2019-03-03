using System;
using System.Collections.Generic;
using System.Text;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal interface ISqlModel
    {
        long Id { get; }
    }
}
