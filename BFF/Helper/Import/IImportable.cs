﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFF.Helper.Import
{
    interface IImportable
    {
        string SavePath { get; set; }

        string Import();
    }
}
