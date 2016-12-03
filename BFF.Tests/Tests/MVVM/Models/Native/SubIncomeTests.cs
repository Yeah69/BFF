﻿using System;
using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.Tests.Mocks.DB;
using NSubstitute;
using Xunit;

namespace BFF.Tests.Tests.MVVM.Models.Native
{
    public static class SubIncomeTests
    {
        public class ConstructionTests
        {
            public static IEnumerable<object[]> SubIncomeData => new[]
            {
                new object[] { 1, 1, 2, "Yeah, Party!", -6969L },
                new object[] { -1, -1, 23, "Long ago", -323L },
                new object[] { 11, 455, -1, "Positive SubIncome", 87978L }
            };

            [Theory, MemberData(nameof(SubIncomeData))]
            public void ConstructionTheory(long id, long parentId, long categoryId, string memo, long sum)
            {
                //Arrange
                SubIncome subIncome = new SubIncome(id, parentId, categoryId, memo, sum);

                //Act

                //Assert
                Assert.Equal(id, subIncome.Id);
                Assert.Equal(parentId, subIncome.ParentId);
                Assert.Equal(categoryId, subIncome.CategoryId);
                Assert.Equal(memo, subIncome.Memo);
                Assert.Equal(sum, subIncome.Sum);
            }

            [Fact]
            public void DefaultConstructionFact()
            {
                //Arrange
                SubIncome subIncome = new SubIncome();

                //Act

                //Assert
                Assert.Equal(-1L, subIncome.Id);
                Assert.Equal(-1L, subIncome.ParentId);
                Assert.Equal(-1L, subIncome.CategoryId);
                Assert.Equal(null, subIncome.Memo);
                Assert.Equal(0L, subIncome.Sum);
            }
        }

        public class CrudTests
        {
            [Fact]
            public void CrudFact()
            {
                //Arrange
                SubIncome subIncome = new SubIncome(1, 1, 2, "Yeah, Party!", -6969L);
                IBffOrm ormMock = BffOrmMoq.Mock;

                //Act
                subIncome.Insert(ormMock);
                subIncome.Update(ormMock);
                subIncome.Delete(ormMock);

                //Assert
                ormMock.Received().Insert(Arg.Any<SubIncome>());
                ormMock.Received().Update(Arg.Any<SubIncome>());
                ormMock.Received().Delete(Arg.Any<SubIncome>());
            }
            [Fact]
            public void NullCrudFact()
            {
                //Arrange
                SubIncome subIncome = new SubIncome(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.Throws<ArgumentNullException>(() => subIncome.Insert(null));
                Assert.Throws<ArgumentNullException>(() => subIncome.Update(null));
                Assert.Throws<ArgumentNullException>(() => subIncome.Delete(null));
            }
        }

        public class PropertyChangedTests
        {
            [Fact]
            public void PropertyChangedFact()
            {
                //Arrange
                SubIncome subIncome = new SubIncome(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.PropertyChanged(subIncome, nameof(subIncome.Id), () => subIncome.Id = 69);
                Assert.PropertyChanged(subIncome, nameof(subIncome.ParentId), () => subIncome.ParentId = 69);
                Assert.PropertyChanged(subIncome, nameof(subIncome.CategoryId), () => subIncome.CategoryId = 23);
                Assert.PropertyChanged(subIncome, nameof(subIncome.Memo), () => subIncome.Memo = "Hangover?");
                Assert.PropertyChanged(subIncome, nameof(subIncome.Sum), () => subIncome.Sum = 69L);
            }

            [Fact]
            public void PropertyNotChangedFact()
            {
                //Arrange
                SubIncome subIncome = new SubIncome(1, 1, 2, "Yeah, Party!", -6969L);

                //Act + Assert
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subIncome, nameof(subIncome.Id), () => subIncome.Id = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subIncome, nameof(subIncome.ParentId), () => subIncome.ParentId = 1));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subIncome, nameof(subIncome.CategoryId), () => subIncome.CategoryId = 2));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subIncome, nameof(subIncome.Memo), () => subIncome.Memo = "Yeah, Party!"));
                Assert.Throws(typeof(Xunit.Sdk.PropertyChangedException),
                    () => Assert.PropertyChanged(subIncome, nameof(subIncome.Sum), () => subIncome.Sum = -6969L));
            }
        }
    }
}