using System;
using System.Threading;

namespace NebulaModel.DataStructures
{
    public sealed class ToggleSwitch
    {
        int onCount;

        public bool Value => onCount > 0;

        public static implicit operator bool(ToggleSwitch toggle) => toggle.Value;

        public Toggle On(bool conditional) => new Toggle(this, conditional ? 1 : 0);
        public Toggle On() => new Toggle(this, 1);

        public readonly struct Toggle : IDisposable
        {
            readonly ToggleSwitch value;
            readonly int count;

            public Toggle(ToggleSwitch value, int count)
            {
                this.value = value;
                this.count = count;

                Interlocked.Add(ref value.onCount, count);
            }

            public void Dispose()
            {
                Interlocked.Add(ref value.onCount, -count);
            }
        }
    }
}
