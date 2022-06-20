using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.GameStates;
using System.Collections.Generic;
using System.IO;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    internal class DysonSphereDataProcessor : PacketProcessor<DysonSphereData>
    {
        public override void ProcessPacket(DysonSphereData packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            switch (packet.Event) 
            {
                case DysonSphereRespondEvent.List:
                    //Overwrite content assigned by UIDETopFunction.SetDysonComboBox()
                    UIComboBox dysonBox = UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.dysonBox;
                    using (BinaryReader br = new BinaryUtils.Reader(packet.BinaryData).BinaryReader)
                    {
                        dysonBox.Items = new List<string>();
                        dysonBox.ItemsData = new List<int>();
                        int count = br.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            int starIndex = br.ReadInt32();
                            dysonBox.Items.Add(GameMain.galaxy.stars[starIndex].displayName);
                            dysonBox.ItemsData.Add(starIndex);
                        }
                    }
                    int index = dysonBox.ItemsData.FindIndex(x => x == UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index);
                    dysonBox.itemIndex = index >= 0 ? index : 0;
                    break;

                case DysonSphereRespondEvent.Load:
                    // The whole fragment is received
                    GameStatesManager.FragmentSize = 0;
                    //Failsafe, if client does not have instantiated sphere for the star, it will create dummy one that will be replaced during import
                    GameMain.data.dysonSpheres[packet.StarIndex] = new DysonSphere();
                    GameMain.data.statistics.production.Init(GameMain.data);
                    //Another failsafe, DysonSphere import requires initialized factory statistics
                    if (GameMain.data.statistics.production.factoryStatPool[0] == null)
                    {
                        GameMain.data.statistics.production.factoryStatPool[0] = new FactoryProductionStat();
                        GameMain.data.statistics.production.factoryStatPool[0].Init();
                    }
                    GameMain.data.dysonSpheres[packet.StarIndex].Init(GameMain.data, GameMain.data.galaxy.stars[packet.StarIndex]);

                    StarData star = GameMain.galaxy.stars[packet.StarIndex];
                    Log.Info($"Parsing {packet.BinaryData.Length} bytes of data for DysonSphere {star.name} (INDEX: {star.id})");
                    using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.BinaryData))
                    {
                        GameMain.data.dysonSpheres[packet.StarIndex].Import(reader.BinaryReader);
                    }
                    if (UIRoot.instance.uiGame.dysonEditor.active)
                    {
                        UIRoot.instance.uiGame.dysonEditor.selection.SetViewStar(GameMain.galaxy.stars[packet.StarIndex]);
                        UIComboBox dysonBox2 = UIRoot.instance.uiGame.dysonEditor.controlPanel.topFunction.dysonBox;
                        int index2 = dysonBox2.ItemsData.FindIndex(x => x == UIRoot.instance.uiGame.dysonEditor.selection.viewStar?.index);
                        dysonBox2.itemIndex = index2 >= 0 ? index2 : 0;
                    }                    
                    if (Multiplayer.Session.IsGameLoaded)
                    {
                        // Don't fade out when client is still joining
                        InGamePopup.FadeOut();
                    }
                    Multiplayer.Session.DysonSpheres.RequestingIndex = -1;
                    Multiplayer.Session.DysonSpheres.IsNormal = true;

                    NebulaModAPI.OnDysonSphereLoadFinished?.Invoke(star.index);
                    break;
                case DysonSphereRespondEvent.Desync:
                    Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                    break;
            }
        }
    }
}
