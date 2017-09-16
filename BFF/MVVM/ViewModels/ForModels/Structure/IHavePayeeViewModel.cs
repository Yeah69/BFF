﻿using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHavePayeeViewModel
    {
        /// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>/// <summary>
        /// Someone or something, who got paid or paid the user by the Transaction/Income.
        /// </summary>
        IReactiveProperty<IPayeeViewModel> Payee { get; }
    }
}