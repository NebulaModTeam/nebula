using UnityEngine;
using Random = System.Random;

namespace NebulaTests.Functional.NebulaNetworking;

// Can probably be moved to parameters
internal static class BandwidthTestSettings
{
    private static Random rand = new Random();

    /// <summary>
    /// Get an incrementing port to allow concurrent testing.
    /// </summary>
    public static int NextPort => rand.Next(8000, 9000);

    public static readonly Bandwidth BandwidthTarget = new(BandwidthOrdinal.Mega, 50);

    public const double BandwidthChallengeDuration = 0.25;

    public const int BandwidthChallengeDurationMs = (int)(BandwidthChallengeDuration * 1000);

    public const int TestTimeout = (int)((BandwidthChallengeDuration + 2) * 1000);
}
