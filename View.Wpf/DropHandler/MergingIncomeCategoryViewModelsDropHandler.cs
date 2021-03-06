﻿using BFF.ViewModel.ViewModels.ForModels;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Threading;
using System.Windows;

namespace BFF.View.Wpf.DropHandler
{
    public class MergingIncomeCategoryViewModelsDropHandler : IDropTarget
    {
        private static readonly Lazy<MergingIncomeCategoryViewModelsDropHandler> Lazy = new(() => new MergingIncomeCategoryViewModelsDropHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static IDropTarget Instance => Lazy.Value;

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IIncomeCategoryViewModel sourceItem 
                && dropInfo.TargetItem is IIncomeCategoryViewModel targetItem 
                && targetItem != sourceItem
                && sourceItem.CanMergeTo(targetItem))
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is IIncomeCategoryViewModel sourceItem 
                && dropInfo.TargetItem is IIncomeCategoryViewModel targetItem)
                sourceItem.MergeTo(targetItem);
        }
    }
}
