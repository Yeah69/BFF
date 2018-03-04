﻿using System;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;

namespace BFF.MVVM.ViewModels
{
    public interface IEditFlagsViewModel
    {
        ReadOnlyObservableCollection<IFlagViewModel> All { get; }

        INewFlagViewModel NewFlagViewModel { get; }
    }

    public class EditFlagsViewModel : ObservableObject, IEditFlagsViewModel, IOncePerBackend
    {
        public INewFlagViewModel NewFlagViewModel { get; }
        public ReadOnlyObservableCollection<IFlagViewModel> All { get; }

        public EditFlagsViewModel(
            IFlagViewModelService service,
            Func<IHaveFlagViewModel, INewFlagViewModel> newFlagViewModelFactory)
        {
            NewFlagViewModel = newFlagViewModelFactory(null);
            All = service.All.ToReadOnlyObservableCollection();
        }
    }
}