using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NebulaWorld.Factory
{
    public class PowerSystemManager
    {
        public static ConcurrentDictionary<int, List<long>> PowerSystemAnimationCache = new ConcurrentDictionary<int, List<long>>();
    }
}
