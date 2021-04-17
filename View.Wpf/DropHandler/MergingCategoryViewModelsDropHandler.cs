using BFF.ViewModel.ViewModels.ForModels;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Threading;
using System.Windows;

namespace BFF.View.Wpf.DropHandler
{
    public class MergingCategoryViewModelsDropHandler : IDropTarget
    {
        private static readonly Lazy<MergingCategoryViewModelsDropHandler> Lazy = new(() => new MergingCategoryViewModelsDropHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static IDropTarget Instance => Lazy.Value;

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ICategoryViewModel sourceItem 
                && dropInfo.TargetItem is ICategoryViewModel targetItem 
                && targetItem != sourceItem
                && sourceItem.CanMergeTo(targetItem))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is ICategoryViewModel sourceItem 
                && dropInfo.TargetItem is ICategoryViewModel targetItem)
                sourceItem.MergeTo(targetItem);
        }
    }
}
