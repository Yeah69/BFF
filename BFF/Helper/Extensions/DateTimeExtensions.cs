using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
