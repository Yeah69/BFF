using System;
using System.Threading.Tasks;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;
using NSubstitute;
using Xunit;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;

namespace BFF.Persistence.Test.Realm.Models.Domain
{
    public class UpdateBudgetCacheForTransactionsTest
    {
        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        public async Task OnTransactionUpdateAsync_CategoryWasNullAndSetToIncomeCategoryWithNegativeOffset_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var afterCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 69L, afterCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryWasNullAndSetToIncomeCategoryWithPositiveOffset_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var afterCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 69L, afterCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasNullAndSetToIncomeCategoryWithZeroAsOffset_DoesntCallForCacheUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var afterCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 0 };

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 69L, afterCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasNullAndSetToCategory_CallForBudgetUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var afterCategory = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 69L, afterCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(afterCategory, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryStaysNull_DoesntCallForCacheUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 69L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_StaysSameIncomeCategory_DoesntCallForCacheUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 3 };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, incomeCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_StaysSameCategory_DoesntCallForCacheUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(category, date, 69L, category, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithNegativeOffsetAndSetToNull_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithPositiveOffsetAndSetToNull_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithZeroAsOffsetAndSetToNull_DoesntCallForCacheUpdateAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 0 };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithNegativeOffsetAndSetToCategory_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };
            var category = new Category { Name = "Test2", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, category, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithPositiveOffsetAndSetToCategory_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };
            var category = new Category { Name = "Test2", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, category, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryWithZeroAsOffsetAndSetToCategory__CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 0 };
            var category = new Category { Name = "Test2", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 69L, category, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13, -3)]
        [InlineData(-13, -1)]
        [InlineData(-13, 0)]
        [InlineData(-13, 1)]
        [InlineData(-13, 3)]
        [InlineData(-13, 13)]
        [InlineData(-3, -13)]
        [InlineData(-3, -1)]
        [InlineData(-3, 0)]
        [InlineData(-3, 1)]
        [InlineData(-3, 3)]
        [InlineData(-3, 13)]
        [InlineData(-1, -13)]
        [InlineData(-1, -3)]
        [InlineData(-1, 0)]
        [InlineData(-1, 1)]
        [InlineData(-1, 3)]
        [InlineData(-1, 13)]
        [InlineData(13, -13)]
        [InlineData(13, -3)]
        [InlineData(13, -1)]
        [InlineData(13, 0)]
        [InlineData(13, 1)]
        [InlineData(13, 3)]
        [InlineData(3, -13)]
        [InlineData(3, -3)]
        [InlineData(3, -1)]
        [InlineData(3, 0)]
        [InlineData(3, 1)]
        [InlineData(3, 13)]
        [InlineData(1, -13)]
        [InlineData(1, -3)]
        [InlineData(1, -1)]
        [InlineData(1, 0)]
        [InlineData(1, 3)]
        [InlineData(1, 13)]
        [InlineData(0, -13)]
        [InlineData(0, -3)]
        [InlineData(0, -1)]
        [InlineData(0, 1)]
        [InlineData(0, 3)]
        [InlineData(0, 13)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryAndSetToOtherIncomeCategory_CallsCacheUpdateAppropriatelyAsync(
            int beforeIncomeMonthOffset,
            int afterIncomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(Math.Min(beforeIncomeMonthOffset, afterIncomeMonthOffset));
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var beforeIncomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = beforeIncomeMonthOffset };
            var afterIncomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = afterIncomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(beforeIncomeCategory, date, 69L, afterIncomeCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13, -13)]
        [InlineData(-3, -3)]
        [InlineData(-1, -1)]
        [InlineData(13, 13)]
        [InlineData(3, 3)]
        [InlineData(1, 1)]
        [InlineData(0, 0)]
        public async Task OnTransactionUpdateAsync_CategoryWasIncomeCategoryAndSetToOtherIncomeCategoryWithSameOffset_DoesntCallCacheUpdateAsync(
            int beforeIncomeMonthOffset,
            int afterIncomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(Math.Min(beforeIncomeMonthOffset, afterIncomeMonthOffset));
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var beforeIncomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = beforeIncomeMonthOffset };
            var afterIncomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = afterIncomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(beforeIncomeCategory, date, 69L, afterIncomeCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasCategoryAndSetToNull_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(category, date, 69L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category ,month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryWasCategoryAndSetToIncomeCategory_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var beforeCategory = new Category { Name = "Test", IsIncomeRelevant = false };
            var afterIncomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(beforeCategory, date, 69L, afterIncomeCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(beforeCategory, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryWasCategoryAndSetToOtherCategory_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var beforeCategory = new Category { Name = "Test", IsIncomeRelevant = false };
            var afterCategory = new Category { Name = "Test2", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(beforeCategory, date, 69L, afterCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(beforeCategory, month).ConfigureAwait(false);
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(afterCategory, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsNullAndEarlierDateToLaterDate_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransactionUpdateAsync(null, earlierDate, 69L, null, laterDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsNullAndLaterDateToEarlierDate_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransactionUpdateAsync(null, laterDate, 69L, null, earlierDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryIsIncomeCategoryAnEarlierDateToLaterDate_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, earlierDate, 69L, incomeCategory, laterDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryIsIncomeCategoryAnLaterDateToEarlierDate_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, laterDate, 69L, incomeCategory, earlierDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsCategoryAndEarlierDateToLaterDate_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(category, earlierDate, 69L, category, laterDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsCategoryAndLaterDateToEarlierDate_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var earlierDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var laterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(category, laterDate, 69L, category, earlierDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsNullAndChangeSum_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransactionUpdateAsync(null, date, 23L, null, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionUpdateAsync_CategoryIsIncomeCategoryAndChangeSum_CallsCacheUpdateAppropriatelyAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test2", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionUpdateAsync(incomeCategory, date, 23L, incomeCategory, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionUpdateAsync_CategoryIsCategoryAndChangeSum_CallsCacheUpdateAppropriatelyAsync()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionUpdateAsync(category, date, 23L, category, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransactionInsertOrDeleteAsync_CategoryIsNull_CallsGlobal_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransactionInsertOrDeleteAsync(null, date).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync( month).ConfigureAwait(false);
        }
        
        [Fact]
        public async Task OnTransactionInsertOrDeleteAsync_CategoryIsCategory_CallsCategorySpecific_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);

            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnTransactionInsertOrDeleteAsync(category, date).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(-13)]
        [InlineData(-3)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(13)]
        public async Task OnTransactionInsertOrDeleteAsync_CategoryIsIncomeCategory_CallsGlobalAsync(int incomeMonthOffset)
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var offsetMonth = month.OffsetMonthBy(incomeMonthOffset);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var incomeCategory = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = incomeMonthOffset };

            // Act
            await sut.OnTransactionInsertOrDeleteAsync(incomeCategory, date).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(offsetMonth).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnBudgetEntryChangeAsync_Vanilla_TrueProxy_Async()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnBudgetEntryChange(category, month).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferInsertOrDeleteAsync_FromAccountNotNullToAccountNotNull_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferInsertOrDeleteAsync(fromAccount, toAccount, month).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferInsertOrDeleteAsync_FromAccountNullToAccountNotNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferInsertOrDeleteAsync(null, toAccount, month).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferInsertOrDeleteAsync_FromAccountNotNullToAccountNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };

            // Act
            await sut.OnTransferInsertOrDeleteAsync(fromAccount, null, month).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferInsertOrDeleteAsync_FromAccountNullToAccountNull_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransferInsertOrDeleteAsync(null, null, month).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnCategoryDeletion_Category_TrueProxy_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnCategoryDeletion(category).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().DeleteCategorySpecificCacheAsync(category).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnCategoryDeletion_IncomeCategory_TrueProxy_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 69 };

            // Act
            await sut.OnCategoryDeletion(category).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().DeleteCategorySpecificCacheAsync(category).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnCategoryMergeForTarget_Category_TrueProxy_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false };

            // Act
            await sut.OnCategoryMergeForTarget(category).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnCategoryMergeForTarget_IncomeCategory_TrueProxy_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = true, IncomeMonthOffset = 69 };

            // Act
            await sut.OnCategoryMergeForTarget(category).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue).ConfigureAwait(false);
        }


        [Fact]
        public async Task OnIncomeCategoryChange_OffsetsAreEqual_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false, IncomeMonthOffset = 23 };

            // Act
            await sut.OnIncomeCategoryChange(category, 23, 23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnIncomeCategoryChange_OffsetsAreUnequal_CallsCacheUpdate_Async()
        {
            // Arrange
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);
            var category = new Category { Name = "Test", IsIncomeRelevant = false, IncomeMonthOffset = 23 };

            // Act
            await sut.OnIncomeCategoryChange(category, 3, 23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNullAndDateChanged_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                null,
                beforeDate,
                23,
                null,
                null,
                afterDate,
                23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNullAndSumChanged_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                null,
                date,
                23,
                null,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNotNullAndDateChanged_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                toAccount,
                beforeDate,
                23,
                fromAccount,
                toAccount,
                afterDate,
                23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNotNullAndSumChanged_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                toAccount,
                date,
                23,
                fromAccount,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceive().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNullToAccountNotNullAndDateChanged_CallsCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                toAccount,
                beforeDate,
                23,
                null,
                toAccount,
                afterDate,
                23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNullToAccountNotNullAndSumChanged_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                toAccount,
                date,
                23,
                null,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNotNullToAccountNullAndDateChanged_CallsCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                null,
                beforeDate,
                23,
                fromAccount,
                null,
                afterDate,
                23).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNotNullToAccountNullAndSumChanged_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                null,
                date,
                23,
                fromAccount,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNotNullAndFromAccountChangedToNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                toAccount,
                date,
                69,
                null,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNotNullAndToAccountChangedToNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                toAccount,
                date,
                69,
                fromAccount,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNullAndToAccountChangedToNotNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                null,
                date,
                69,
                null,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_BothAccountsNullAndFromAccountChangedToNotNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                null,
                date,
                69, 
                fromAccount,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNotNullToAccountNullAndToAccountChangedToNotNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                null,
                date,
                69,
                fromAccount,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNotNullToAccountNullAndFromAccountChangedToNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };

            // Act
            await sut.OnTransferUpdateAsync(
                fromAccount,
                null,
                date,
                69,
                null,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNullToAccountNotNullAndFromAccountChangedToNotNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var fromAccount = new Account { Name = "From" };
            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                toAccount,
                date,
                69,
                fromAccount,
                toAccount,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnTransferUpdateAsync_FromAccountNullToAccountNotNullAndToAccountChangedToNull_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            var toAccount = new Account { Name = "To" };

            // Act
            await sut.OnTransferUpdateAsync(
                null,
                toAccount,
                date,
                69,
                null,
                null,
                date,
                69).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnAccountUpdateAsync_BeforeDateEarlierThanAfterDate_CallsCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnAccountChange(beforeDate, 69L, afterDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnAccountUpdateAsync_BeforeDateLaterThanAfterDate_CallsCacheUpdate_Async()
        {
            // Arrange
            var beforeDate = new DateTimeOffset(2023, 9, 6, 0, 0, 0, TimeSpan.Zero);
            var afterDate = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnAccountChange(beforeDate, 69L, afterDate, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnAccountUpdateAsync_ChangedBalance_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnAccountChange(date, 3L, date, 23L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnAccountUpdateAsync_NothingChanges_DoesntCallCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnAccountChange(date, 69L, date, 69L).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.DidNotReceiveWithAnyArgs().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }

        [Fact]
        public async Task OnAccountUpdateAsync_NothingChanges_CallsCacheUpdate_Async()
        {
            // Arrange
            var date = new DateTimeOffset(2023, 6, 9, 0, 0, 0, TimeSpan.Zero);
            var month = new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var mockBudgetOrm = Substitute.For<IBudgetOrm>();
            var sut = new UpdateBudgetCache(
                mockBudgetOrm);

            // Act
            await sut.OnAccountInsertOrDelete(date).ConfigureAwait(false);

            // Assert
            await mockBudgetOrm.Received().UpdateGlobalPotCacheAsync(month).ConfigureAwait(false);
        }
    }
}
