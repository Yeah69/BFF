﻿using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BFF.Model.Native.Structure
{
    interface IParentTransInc<T> : ITransInc where T : ISubTransInc 
    {
        ObservableCollection<T> SubElements { get; }
        ObservableCollection<T> NewSubElements { get; }
        ICommand OpenParentTitView { get; }
    }
}
