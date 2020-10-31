using System;
using System.Threading;
using System.Windows;
using BFF.ViewModel.ViewModels.ForModels;
using GongSolutions.Wpf.DragDrop;

namespace BFF.View.DropHandler
{
    public class MergingPayeeViewModelsDropHandler : IDropTarget
    {
        private static readonly Lazy<MergingPayeeViewModelsDropHandler> Lazy = new Lazy<MergingPayeeViewModelsDropHandler>(() => new MergingPayeeViewModelsDropHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static IDropTarget Instance => Lazy.Value;

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IPayeeViewModel sourceItem 
                && dropInfo.TargetItem is IPayeeViewModel targetItem 
                && targetItem != sourceItem
                && sourceItem.CanMergeTo(targetItem))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IPayeeViewModel sourceItem 
                && dropInfo.TargetItem is IPayeeViewModel targetItem)
                sourceItem.MergeTo(targetItem);
        }
    }
}
