using System;

namespace BFF.Model.Native.Structure
{
    public abstract class TitBase : TitLike
    {
        public abstract DateTime Date { get; set; }

        public abstract string Memo { get; set; }

        public abstract long? Sum { get; set; }

        public abstract bool Cleared { get; set; }

        public abstract string Type { get; set; }

        public TitBase()
        {
        }
    }
}
