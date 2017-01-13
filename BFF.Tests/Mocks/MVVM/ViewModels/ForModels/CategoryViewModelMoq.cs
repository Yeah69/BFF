using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Tests.Mocks.MVVM.Models.Native;
using NSubstitute;

namespace BFF.Tests.Mocks.MVVM.ViewModels.ForModels
{
    public static class CategoryViewModelMoq
    {
        public static IList<ICategoryViewModel> Mocks
        {
            get
            {
                IList<ICategoryViewModel> categories = new List<ICategoryViewModel>();
                InitializeCategories();
                SetCategoriesParents();
                SetCategoriesChildCategories();
                return categories;

                void InitializeCategories()
                {
                    foreach(var categorySetTuple in CategoryMoq.CategorySet)
                    {
                        categories.Add(CreateMock(categorySetTuple.Id,
                                                  categorySetTuple.Name,
                                                  categorySetTuple.ParentId));
                    }
                }

                void SetCategoriesParents()
                {
                    foreach(var categoryViewModel in categories)
                    {
                        long? parentId = CategoryMoq.CategorySet.Single(cs => cs.Id == categoryViewModel.Id).ParentId;
                        var parent = parentId == null
                                         ? null
                                         : categories.Single(cvw => cvw.Id == parentId);
                        categoryViewModel.Parent.Returns(parent);
                    }
                }

                void SetCategoriesChildCategories()
                {
                    foreach(var categoryViewModel in categories)
                    {
                        IEnumerable<long> childIds = CategoryMoq.CategorySet.Where(cs => cs.Item3 == categoryViewModel.Id).Select(cs => cs.Item1);
                        IList<ICategoryViewModel> children = new List<ICategoryViewModel>();
                        foreach(var childId in childIds)
                        {
                            children.Add(categories.Single(cvm => cvm.Id == childId));
                        }

                        categoryViewModel.Categories.Returns(new ObservableCollection<ICategoryViewModel>(children));
                    }
                }
            }
        }

        private static ICategoryViewModel CreateMock(long id, string name, long? parentId)
        {
            var mock = Substitute.For<ICategoryViewModel>();

            mock.Id.Returns(id);
            mock.Name.Returns(name);
            /*if(parentId != null)
            {
                ICategoryViewModel parent = CreateMock(CategorySet[(int) parentId].Id, CategorySet[(int) parentId].Name,
                                                       CategorySet[(int) parentId - 1].ParentId);
                mock.Parent.Returns(parent);

            }
            else
                mock.Parent.Returns(default(ICategoryViewModel));*/

            return mock;
        }
    }
}