using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaWorld;
using NebulaWorld.GameStates;
using System;
using UnityEngine;

namespace NebulaNetwork.PacketProcessors.GameStates
{
    [RegisterPacketProcessor]
    public class GameStateUpdateProcessor : PacketProcessor<GameStateUpdate>
    {
        public float BUFFERING_TIME = 30f;
        public float BUFFERING_TICK = 60f;

        private int averageRTT;
        private float avaerageUPS = 60f;
        private bool hasChanged;

        public override void ProcessPacket(GameStateUpdate packet, NebulaConnection conn)
        {
            long rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.SentTime;
            averageRTT = (int)(averageRTT * 0.8 + rtt * 0.2);
            avaerageUPS = avaerageUPS * 0.8f + packet.UnitsPerSecond * 0.2f;
            Multiplayer.Session.World.UpdatePingIndicator($"Ping: {averageRTT}ms");

            // We offset the tick received to account for the time it took to receive the packet
            long tickOffsetSinceSent = (long)Math.Round(packet.UnitsPerSecond * rtt / 2 / 1000);
            long currentGameTick = packet.GameTick + tickOffsetSinceSent;
            long diff = currentGameTick - GameMain.gameTick;

            // Discard abnormal packet (usually after host saving the file)
            if (rtt > 2 * averageRTT || avaerageUPS - packet.UnitsPerSecond > 15)
            {
                // Initial connetion
                if (GameMain.gameTick < 1200L)
                {
                    averageRTT = (int)rtt;
                    GameMain.gameTick = currentGameTick;
                }
                Log.Debug($"GameStateUpdate unstable. RTT:{rtt}(avg{averageRTT}) UPS:{packet.UnitsPerSecond:F2}(avg{avaerageUPS:F2})");
                return;
            }

            if (!Config.Options.SyncUps)
            {
                // We allow for a small drift of 5 ticks since the tick offset using the ping is only an approximation
                if (GameMain.gameTick > 0 && Mathf.Abs(diff) > 5)
                {
                    Log.Info($"Game Tick got updated since it was desynced, was {GameMain.gameTick}, diff={diff}");
                    GameMain.gameTick = currentGameTick;
                }
                // Reset FixUPS when user turns off the option
                if (hasChanged)
                {
                    FPSController.SetFixUPS(0);
                    hasChanged = false;
                }
                return;
            }

            // Adjust client's UPS to match game tick with server, range 30~120 UPS
            float UPS = diff / 1f + avaerageUPS;
            long skipTick = 0;
            if (UPS > GameStatesManager.MaxUPS)
            {
                // Try to disturbute game tick difference into BUFFERING_TIME (seconds)
                if (diff / BUFFERING_TIME + avaerageUPS > GameStatesManager.MaxUPS)
                {
                    // The difference is too large, need to skip ticks to catch up
                    skipTick = (long)(UPS - GameStatesManager.MaxUPS);
                }
                UPS = GameStatesManager.MaxUPS;
            }
            else if (UPS < GameStatesManager.MinUPS)
            {
                if (diff + avaerageUPS - GameStatesManager.MinUPS < -BUFFERING_TICK)
                {
                    skipTick = (long)(UPS - GameStatesManager.MinUPS);
                }
                UPS = GameStatesManager.MinUPS;
            }
            if (skipTick != 0)
            {
                Log.Info($"Game Tick was desynced. skip={skipTick} diff={diff,2}, RTT={rtt}ms, UPS={packet.UnitsPerSecond:F2}");
                GameMain.gameTick += skipTick;
            }
            FPSController.SetFixUPS(UPS);
            hasChanged = true;
            // Tick difference in the next second. Expose for other mods
            GameStatesManager.NotifyTickDifference((diff / 1f + avaerageUPS) - UPS);
        }
    }
}
