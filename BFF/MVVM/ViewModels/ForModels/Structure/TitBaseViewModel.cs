using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public abstract class TitBaseViewModel : TitLikeViewModel
    {
        public abstract DateTime Date { get; set; }
        public abstract bool Cleared { get; set; }

        protected TitBaseViewModel(IBffOrm orm) : base(orm) { }
    }
}
