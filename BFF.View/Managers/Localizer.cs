﻿using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.View.Extensions;

namespace BFF.View.Managers
{
    class Localizer : ILocalizer, IOncePerApplication
    {
        public string Localize(string key) => key.Localize();

        public T Localize<T>(string key) => key.Localize<T>();
    }
}