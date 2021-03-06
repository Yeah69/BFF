﻿namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface IHaveCategoryViewModel
    {
        ICategoryBaseViewModel? Category { get; set; }

        INewCategoryViewModel NewCategoryViewModel { get; }
    }
}
