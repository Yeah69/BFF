﻿namespace BFF.MVVM.ViewModels
{
    public interface IEmptyViewModel
    {
    }

    public class EmptyContentViewModel : SessionViewModelBase, IEmptyViewModel
    {
        protected override void OnIsOpenChanged(bool isOpen)
        {
            
        }
    }
}
