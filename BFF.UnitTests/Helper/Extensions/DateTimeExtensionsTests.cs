using System;
using System.Collections.Generic;
using BFF.Helper.Extensions;
using Xunit;

namespace BFF.Tests.Helper.Extensions
{
    class DateTimeExtensionsTests
    {
        public static IEnumerable<object[]> TestDataFor_OffsetMonthBy_CurrentMonthAndOffset_ResultIsMonthAfterOffset()
        {
            yield return new object[] { new DateTime(2018, 1, 23), 0, new DateTime(2018, 1, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 5, new DateTime(2018, 6, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 11, new DateTime(2018, 12, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 12, new DateTime(2019, 1, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 15, new DateTime(2019, 4, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 23, new DateTime(2019, 12, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 24, new DateTime(2020, 1, 1) };
            yield return new object[] { new DateTime(2018, 1, 23), 30, new DateTime(2019, 7, 1) };
        }

        [Theory]
        [MemberData(nameof(TestDataFor_OffsetMonthBy_CurrentMonthAndOffset_ResultIsMonthAfterOffset), MemberType = typeof(DateTimeExtensionsTests))]
        public void OffsetMonthBy_CurrentMonthAndOffset_ResultIsMonthAfterOffset(DateTime current, int offset, DateTime expected)
        {
            // Arrange

            // Act
            var result = current.OffsetMonthBy(offset);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
