using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NebulaModel.DataStructures
{
    public sealed class ToggleSwitch
    {
        int onCount;

        public static implicit operator bool(ToggleSwitch toggle) => toggle.onCount > 0;

        public Toggle On() => new Toggle(this);

        public readonly struct Toggle : IDisposable
        {
            readonly ToggleSwitch value;
            public Toggle(ToggleSwitch value)
            {
                this.value = value;

                Interlocked.Increment(ref value.onCount);
            }

            public void Dispose()
            {
                Interlocked.Decrement(ref value.onCount);
            }
        }
    }
}
