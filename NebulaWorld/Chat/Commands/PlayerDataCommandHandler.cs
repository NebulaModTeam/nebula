#region

using NebulaWorld.MonoBehaviours.Local.Chat;
using System.Collections.Generic;
using NebulaAPI.GameState;
using HarmonyLib;
using NebulaModel.DataStructures.Chat;
using System.IO;
using NebulaModel;
using NebulaModel.Logger;
using NebulaAPI.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaWorld.Chat.Commands;

public class PlayerDataCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 1)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }

        // Due to dependency order, get SaveManager.playerSaves by reflection
        var saveManagerType = AccessTools.TypeByName("NebulaNetwork.SaveManager");
        var playerSaves = (Dictionary<string, IPlayerData>)AccessTools.Field(saveManagerType, "playerSaves").GetValue(null);

        switch (parameters[0])
        {
            case "list":
                {
                    var resp = $"Player count in .server file: {playerSaves.Count}\n";
                    foreach (var pair in playerSaves)
                    {
                        resp += $"[{pair.Key.Substring(0, 5)}] {pair.Value.Username}";
                    }
                    window.SendLocalChatMessage(resp, ChatMessageType.CommandOutputMessage);
                    break;
                }
            case "load" when parameters.Length < 2:
                throw new ChatCommandUsageException("Need to specifiy hash string or name of a player!");
            case "load":
                {
                    var input = parameters[1];
                    foreach (var pair in playerSaves)
                    {
                        if (input == pair.Key.Substring(0, input.Length) || input == pair.Value.Username)
                        {
                            window.SendLocalChatMessage($"Load [{pair.Key.Substring(0, 5)}] {pair.Value.Username}", ChatMessageType.CommandOutputMessage);
                            LoadPlayerData(pair.Value);
                            return;
                        }
                    }
                    break;
                }
            case "remove" when parameters.Length < 2:
                throw new ChatCommandUsageException("Need to specifiy hash string or name of a player!");
            case "remove":
                {
                    var input = parameters[1];
                    var removeHash = "";
                    foreach (var pair in playerSaves)
                    {
                        if (input == pair.Key.Substring(0, input.Length) || input == pair.Value.Username)
                        {
                            window.SendLocalChatMessage($"Remove [{pair.Key.Substring(0, 5)}] {pair.Value.Username}", ChatMessageType.CommandOutputMessage);
                            removeHash = pair.Key;
                            break;
                        }
                    }
                    playerSaves.Remove(removeHash);
                    break;
                }
        }
    }

    public string GetDescription()
    {
        return "Manage the stored multiplayer player data".Translate();
    }

    public string[] GetUsage()
    {
        return ["list", "load <hashString>", "remove <hashString>"];
    }

    static void LoadPlayerData(IPlayerData playerData)
    {
        Log.Info($"Teleporting to target planet {GameMain.localPlanet?.id ?? 0} => {playerData.LocalPlanetId}");
        var actionSail = GameMain.mainPlayer.controller.actionSail;
        if (playerData.LocalPlanetId > 0)
        {
            if (playerData.LocalPlanetId == GameMain.localPlanet?.id)
            {
                UIRoot.instance.uiGame.globemap.LocalPlanetTeleport(playerData.LocalPlanetPosition.ToVector3());
            }
            else
            {
                var destPlanet = GameMain.galaxy.PlanetById(playerData.LocalPlanetId);
                actionSail.fastTravelTargetPlanet = destPlanet;
                actionSail.fastTravelTargetLPos = playerData.LocalPlanetPosition.ToVector3();
                actionSail.fastTravelTargetUPos = destPlanet.uPosition + (VectorLF3)(destPlanet.runtimeRotation * actionSail.fastTravelTargetLPos);
                actionSail.StartFastTravelToUPosition(actionSail.fastTravelTargetUPos);
            }
        }
        else
        {
            GameMain.mainPlayer.controller.actionSail.StartFastTravelToUPosition(playerData.UPosition.ToVectorLF3());
        }

        var mechaData = playerData.Mecha;
        Log.Info("Loading player inventory...");
        using (var ms = new MemoryStream())
        {
            var bw = new BinaryWriter(ms);
            mechaData.Inventory.Export(bw);
            mechaData.DeliveryPackage.Export(bw);
            mechaData.WarpStorage.Export(bw);

            ms.Seek(0, SeekOrigin.Begin);
            var br = new BinaryReader(ms);
            GameMain.mainPlayer.package.Import(br);
            GameMain.mainPlayer.deliveryPackage.Import(br);
            GameMain.mainPlayer.mecha.warpStorage.Import(br);
        }
        if (!Config.Options.SyncSoil)
        {
            GameMain.mainPlayer.sandCount = mechaData.SandCount;
        }
        GameMain.mainPlayer.mecha.coreEnergy = mechaData.CoreEnergy;
        GameMain.mainPlayer.mecha.reactorEnergy = mechaData.ReactorEnergy;

        if (playerData.Appearance != null)
        {
            Log.Info("Loading custom appearance...");
            playerData.Appearance.CopyTo(GameMain.mainPlayer.mecha.appearance);
            GameMain.mainPlayer.mechaArmorModel.RefreshAllPartObjects();
            GameMain.mainPlayer.mechaArmorModel.RefreshAllBoneObjects();
            GameMain.mainPlayer.mecha.appearance.NotifyAllEvents();

            var editor = UIRoot.instance.uiMechaEditor;
            editor.selection.ClearSelection();
            editor.saveGroup._Close();
            if (editor.mecha.diyAppearance == null)
            {
                editor.mecha.diyAppearance = new MechaAppearance();
                editor.mecha.diyAppearance.Init();
            }
            GameMain.mainPlayer.mecha.appearance.CopyTo(editor.mecha.diyAppearance);
            editor.mechaArmorModel.RefreshAllPartObjects();
            editor.mechaArmorModel.RefreshAllBoneObjects();
            editor.mecha.diyAppearance.NotifyAllEvents();
            editor._left_content_height_max = 0f;
            editor.SetLeftScrollTop();
            editor.saveGroup._Open();

            using var writer = new BinaryUtils.Writer();
            GameMain.mainPlayer.mecha.appearance.Export(writer.BinaryWriter);
            Multiplayer.Session.Network.SendPacket(new PlayerMechaArmor(Multiplayer.Session.LocalPlayer.Id,
                writer.CloseAndGetBytes()));
        }
    }
}
