using HarmonyLib;
using NebulaModel.Packets.Players;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(MechaLab))]
    internal class MechaLab_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(MechaLab.GameTick))]
        public static IEnumerable<CodeInstruction> GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
             * Syncs client's hashrate (num4), and adds hashrate to host's side
             * Before
             * if (num4 > 0)
            */
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Blt),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ble)
                );

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("MechaLab.GameTick_Transpiler failed. Mod version not compatible with game version");
                return instructions;
            }

            CodeInstruction num4Inst = matcher.InstructionAt(1);
            return matcher
                   .Advance(1)
                   .InsertAndAdvance(num4Inst)
                   .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int>>(hashRate =>
                   {
                       if (Multiplayer.IsActive)
                       {
                           // For client, sync hashRate if it changed
                           if (Multiplayer.Session.LocalPlayer.IsClient)
                           {
                               NebulaAPI.IMechaData mechaData = Multiplayer.Session.LocalPlayer.Data.Mecha;
                               if (hashRate != mechaData.ResearchHashRate)
                               {
                                   NebulaModel.Logger.Log.Info($"Sending hashRate of {hashRate}");
                                   mechaData.ResearchHashRate = hashRate;
                                   Multiplayer.Session.Network.SendPacket(new PlayerMechaData(GameMain.mainPlayer, hashRate));
                               }
                               return;
                           }

                           // For host, add client's hash rates
                           if (Multiplayer.Session.LocalPlayer.IsHost)
                           {
                               NebulaAPI.IPlayerData[] playerDataArr = Multiplayer.Session.Network.PlayerManager.GetAllPlayerDataIncludingHost();
                               for (int i = 0; i < playerDataArr.Length; i++)
                               {
                                   if (playerDataArr[i].PlayerId != Multiplayer.Session.LocalPlayer.Id)
                                   {
                                       hashRate += playerDataArr[i].Mecha.ResearchHashRate;
                                   }
                               }
                               return;
                           }
                       }
                   }))
                   .InstructionEnumeration();
        }
    }
}
