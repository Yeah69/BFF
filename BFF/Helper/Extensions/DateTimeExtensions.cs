using System;

namespace BFF.Helper.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime PreviousMonth(this DateTime @this)
        {
            return new DateTime(
                @this.Month != 1 ? @this.Year : @this.Year - 1,
                @this.Month != 1 ? @this.Month - 1 : 12,
                1);
        }

        public static DateTime NextMonth(this DateTime @this)
        {
            return new DateTime(
                @this.Month != 12 ? @this.Year : @this.Year + 1,
                @this.Month != 12 ? @this.Month + 1 : 1,
                1);
        }

        public static DateTime OffsetMonthBy(this DateTime @this, int offset)
        {
            if(offset == 0)
                return new DateTime(
                    @this.Year,
                    @this.Month,
                    1);
            if (offset > 0)
            {
                var current = new DateTime(@this.Year + offset / 12, @this.Month, 1);
                var limit = offset % 12;
                for (int i = 1; i <= limit; i++)
                    current = current.NextMonth();
                return current;
            }
            else
            {
                offset *= -1;

                var current = new DateTime(@this.Year - offset / 12, @this.Month, 1);
                var limit = offset % 12;
                for (int i = 1; i <= limit; i++)
                    current = current.PreviousMonth();
                return current;
            }
        }
    }
}
