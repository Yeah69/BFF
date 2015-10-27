﻿using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native.Structure
{
    abstract class TransItemBase : DataModelBase
    {
        public abstract long Id { get; set; }

        public abstract DateTime Date { get; set; }

        public abstract string Memo { get; set; }

        public abstract double? Sum { get; set; }

        public abstract bool Cleared { get; set; }

        public abstract string Type { get; set; }
    }
}
