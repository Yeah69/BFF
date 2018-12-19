using System;
using System.Threading;
using System.Windows;
using BFF.ViewModel.ViewModels.ForModels;
using GongSolutions.Wpf.DragDrop;

namespace BFF.DropHandler
{
    public class MergingFlagViewModelsDropHandler : IDropTarget
    {
        private static readonly Lazy<MergingFlagViewModelsDropHandler> Lazy = new Lazy<MergingFlagViewModelsDropHandler>(() => new MergingFlagViewModelsDropHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static IDropTarget Instance => Lazy.Value;

        private MergingFlagViewModelsDropHandler()
        {
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IFlagViewModel sourceItem 
                && dropInfo.TargetItem is IFlagViewModel targetItem 
                && targetItem != sourceItem
                && sourceItem.CanMergeTo(targetItem))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            IFlagViewModel sourceItem = dropInfo.Data as IFlagViewModel;
            IFlagViewModel targetItem = dropInfo.TargetItem as IFlagViewModel;
            sourceItem?.MergeTo(targetItem);
        }
    }
}
