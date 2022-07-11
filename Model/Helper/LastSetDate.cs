using System;
using BFF.Core.IoC;

namespace BFF.Model.Helper
{
    public interface ILastSetDate : IScopeInstance
    {
        DateTime Date { get; set; }
    }

    internal class LastSetDate : ILastSetDate
    {
        public DateTime Date { get; set; } = DateTime.Today;
    }
}
