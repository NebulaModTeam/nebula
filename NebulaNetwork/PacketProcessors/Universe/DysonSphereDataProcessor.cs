#region

using System;
using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.GameStates;

#endregion

namespace NebulaNetwork.PacketProcessors.Universe;

[RegisterPacketProcessor]
internal class DysonSphereDataProcessor : PacketProcessor<DysonSphereData>
{
    protected override void ProcessPacket(DysonSphereData packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        switch (packet.Event)
        {
            case DysonSphereRespondEvent.List:
                //Overwrite content assigned by UIDETopFunction.SetDysonComboBox()
                var dysonBox = UIRoot.instance?.uiGame?.dysonEditor?.controlPanel?.topFunction?.dysonBox;
                if (dysonBox != null)
                {
                    using (var br = new BinaryUtils.Reader(packet.BinaryData).BinaryReader)
                    {
                        dysonBox.Items = [];
                        dysonBox.ItemsData = [];
                        var count = br.ReadInt32();
                        for (var i = 0; i < count; i++)
                        {
                            var starIndex = br.ReadInt32();
                            dysonBox.Items.Add(GameMain.galaxy.stars[starIndex].displayName);
                            dysonBox.ItemsData.Add(starIndex);
                        }
                    }
                    var index = dysonBox.ItemsData.FindIndex(x =>
                        x == UIRoot.instance?.uiGame?.dysonEditor?.selection?.viewStar?.index);
                    dysonBox.itemIndex = index >= 0 ? index : 0;
                }
                break;

            case DysonSphereRespondEvent.Load:
                // The whole fragment is received
                GameStatesManager.FragmentSize = 0;
                //Failsafe: if client already has an instantiated sphere for the star, free its resources cleanly
                if (GameMain.data.dysonSpheres[packet.StarIndex] != null)
                {
                    try
                    {
                        GameMain.data.dysonSpheres[packet.StarIndex].Free();
                    }
                    catch (Exception e)
                    {
                        Log.Warn($"Exception while freeing existing dyson sphere {packet.StarIndex}: {e}");
                    }
                }
                GameMain.data.dysonSpheres[packet.StarIndex] = new DysonSphere();
                GameMain.data.statistics.production.Init(GameMain.data);
                //Another failsafe, DysonSphere import requires initialized factory statistics
                if (GameMain.data.statistics.production.factoryStatPool[0] == null)
                {
                    GameMain.data.statistics.production.factoryStatPool[0] = new FactoryProductionStat();
                    GameMain.data.statistics.production.factoryStatPool[0].Init();
                }
                var dysonSphere = GameMain.data.dysonSpheres[packet.StarIndex];
                var star = GameMain.galaxy.stars[packet.StarIndex];
                using (Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket.On())
                using (Multiplayer.Session.DysonSpheres.IsIncomingRequest.On())
                {
                    dysonSphere.Init(GameMain.data, GameMain.data.galaxy.stars[packet.StarIndex]);
                    dysonSphere.ResetNew();

                    Log.Info($"Parsing {packet.BinaryData.Length} bytes of data for DysonSphere {star.name} (INDEX: {star.id})");
                    using (var reader = new BinaryUtils.Reader(packet.BinaryData))
                    {
                        dysonSphere.Import(reader.BinaryReader);
                    }
                }

                // Ensure render masks and models are active and rendered
                dysonSphere.inGameRenderMaskS = -1;
                dysonSphere.inGameRenderMaskL = -1;
                dysonSphere.inEditorRenderMaskS = -1;
                dysonSphere.inEditorRenderMaskL = -1;

                if (UIRoot.instance?.uiGame?.dysonEditor != null && UIRoot.instance.uiGame.dysonEditor.active)
                {
                    DysonSphere.renderPlace = ERenderPlace.Dysonmap;
                }
                else if (UIRoot.instance?.uiGame?.starmap != null && UIRoot.instance.uiGame.starmap.active)
                {
                    DysonSphere.renderPlace = ERenderPlace.Starmap;
                }
                else
                {
                    DysonSphere.renderPlace = ERenderPlace.Universe;
                }

                if (dysonSphere.nrdCapacity > 0 && dysonSphere.nrdBuffer == null)
                {
                    dysonSphere.SetNrdCapacity(dysonSphere.nrdCapacity);
                }

                dysonSphere.LayerSort();
                if (dysonSphere.layersSorted != null)
                {
                    for (var i = 0; i < dysonSphere.layersSorted.Length; i++)
                    {
                        var layer = dysonSphere.layersSorted[i];
                        if (layer?.shellPool == null) continue;
                        for (var j = 1; j < layer.shellCursor; j++)
                        {
                            var shell = layer.shellPool[j];
                            if (shell != null && shell.id == j && (shell.mesh == null || shell.material == null))
                            {
                                shell.GenerateModelObjects();
                            }
                        }
                    }
                }

                dysonSphere.modelRenderer?.RebuildModels();
                dysonSphere.swarm?.CalibrateOrbitCursor();
                dysonSphere.swarm?.SetOrbitColorBuffer();

                Multiplayer.Session.DysonSpheres.LoadedSpheres.Add(packet.StarIndex);
                try
                {
                    if (UIRoot.instance?.uiGame?.dysonEditor != null && UIRoot.instance.uiGame.dysonEditor.active)
                    {
                        var selection = UIRoot.instance.uiGame.dysonEditor.selection;
                        selection?.SetViewStar(GameMain.galaxy.stars[packet.StarIndex]);
                        var dysonBox2 = UIRoot.instance.uiGame.dysonEditor.controlPanel?.topFunction?.dysonBox;
                        if (dysonBox2?.ItemsData != null && dysonBox2.ItemsData.Count > 0)
                        {
                            var index2 = dysonBox2.ItemsData.FindIndex(x =>
                                x == selection?.viewStar?.index);
                            dysonBox2.itemIndex = index2 >= 0 ? index2 : 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"Error updating dyson editor UI after load: {e}");
                }
                finally
                {
                    Multiplayer.Session.DysonSpheres.RequestingIndex = -1;
                    Multiplayer.Session.DysonSpheres.IsNormal = true;
                }

                if (Multiplayer.Session.IsGameLoaded)
                {
                    // Don't fade out when client is still joining
                    InGamePopup.FadeOut();
                }

                try
                {
                    NebulaModAPI.OnDysonSphereLoadFinished?.Invoke(star.index);
                }
                catch (Exception e)
                {
                    Log.Error("NebulaModAPI.OnDysonSphereLoadFinished error:\n" + e);
                }
                break;
            case DysonSphereRespondEvent.Desync:
                Multiplayer.Session.DysonSpheres.HandleDesync(packet.StarIndex, conn);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(packet), "Unknown DysonSphereRespondEvent: " + packet.Event);
        }
    }
}
