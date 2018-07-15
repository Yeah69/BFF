﻿using System;
using System.Threading;
using System.Windows;
using BFF.MVVM.ViewModels.ForModels;
using GongSolutions.Wpf.DragDrop;

namespace BFF.MVVM.DropHandler
{
    public class MergingIncomeCategoryViewModelsDropHandler : IDropTarget
    {
        private static readonly Lazy<MergingIncomeCategoryViewModelsDropHandler> Lazy = new Lazy<MergingIncomeCategoryViewModelsDropHandler>(() => new MergingIncomeCategoryViewModelsDropHandler(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static IDropTarget Instance => Lazy.Value;

        private MergingIncomeCategoryViewModelsDropHandler()
        {
        }

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
            IIncomeCategoryViewModel sourceItem = dropInfo.Data as IIncomeCategoryViewModel;
            IIncomeCategoryViewModel targetItem = dropInfo.TargetItem as IIncomeCategoryViewModel;
            sourceItem?.MergeTo(targetItem);
        }
    }
}