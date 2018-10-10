using System;
using BFF.Core.IoCMarkerInterfaces;
using BFF.DB;

namespace BFF.Helper
{
    public interface ILastSetDate : IOncePerBackend
    {
        DateTime Date { get; set; }
    }

    public class LastSetDate : ILastSetDate
    {
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
