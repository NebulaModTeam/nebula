using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereAddSailBulletProcessor : PacketProcessor<DysonSphereBulletCorrectionPacket>
    {
        public override void ProcessPacket(DysonSphereBulletCorrectionPacket packet, NebulaConnection conn)
        {
            //Check if the bullet that needs to be corrected exists
            if (GameMain.data.dysonSpheres[packet.StarIndex]?.swarm?.bulletPool[packet.BulletId] != null)
            {
                //Update destination values for the bullet
                SailBullet bullet = GameMain.data.dysonSpheres[packet.StarIndex].swarm.bulletPool[packet.BulletId];
                bullet.uEnd = packet.UEnd.ToVector3();
                bullet.uEndVel = packet.UEndVel.ToVector3();
            }
            else
            {
                //TODO: Maybe queue it and check next frame if the bullet already exist?
                //Note: this situation was not observed during test, but maybe it can due to the severe lags?
            }
        }
    }
}
