﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface ICategoryOrm : IOncePerBackend
    {
        Task<IEnumerable<ICategoryRealm>> ReadCategoriesAsync();
        Task<IEnumerable<ICategoryRealm>> ReadIncomeCategoriesAsync();
    }
}