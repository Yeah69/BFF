using System.Diagnostics.CodeAnalysis;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.ViewModels.ForModels.Structure
{
    [SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded")]
    public abstract class DataModelViewModelTests<T> where T : DataModelViewModel
    {
        protected abstract (T, IDataModel, IBffOrm) CreateDataModelViewModel(long modelId, ICommonPropertyProvider commonPropertyProvider =  null);

        [Fact]
        public void IdGet_CallsModelIdGet()
        {
            //Arrange
            var (viewModel, modelMock, _) = CreateDataModelViewModel(69);

            //Act
            _ = viewModel.Id;

            //Assert
            _ = modelMock.Received().Id;
        }
    }
}