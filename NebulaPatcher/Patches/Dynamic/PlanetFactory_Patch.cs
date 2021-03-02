using HarmonyLib;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;

namespace NebulaPatcher.Patches.Dynamic
{
	[HarmonyPatch(typeof(PlayerAction_Mine), "GameTick")]
	class PlanetFactory_Patch
	{
		public static void Prefix(PlayerAction_Mine __instance, long timei)
		{
			/*
			 * i go this longer route as i want to include mining information of veins and want to play the destroy animation of "vegetables"
			 * NOTE: there is GameMain.mainPlayer.factory.planet.physics.NotifyObjectRemove() and GameMain.gameScenario.NotifyOnVegetableMined()
			 * but the latter is not called for veins and the first does not contain the information needed to play the animation.
			 */
			if(__instance.player.factory == null)
            {
				return;
            }

			int miningID = 0;
			EObjectType miningType = EObjectType.Entity;

			double energyGet;
			float ratio; // we are only interested in this
			__instance.player.mecha.QueryEnergy(__instance.player.mecha.miningPower * 0.01666666753590107, out energyGet, out ratio);
			int timeMined = (int)(__instance.player.mecha.miningSpeed * ratio * 10000f + 0.49f); // to use it in here so we can compute the time the player is already mining

			if (__instance.player.currentOrder != null && __instance.player.currentOrder.type == EOrderType.Mine && __instance.player.currentOrder.targetReached)
            {
				miningID = __instance.player.currentOrder.objId;
				miningType = __instance.player.currentOrder.objType;
            }

			if(miningType == EObjectType.Entity || (miningType == EObjectType.Vegetable && __instance.player.factory.GetVegeData(miningID).id == 0))
            {
				miningID = 0;
            }
			if(miningType == EObjectType.Vein)
            {
				if(__instance.player.factory.GetVeinData(miningID).id == 0 || __instance.player.factory.GetVeinData(miningID).type == EVeinType.Oil)
                {
					miningID = 0;
                }
            }

			if(miningID != 0 && miningType == EObjectType.Vegetable)
            {
				VegeData vData = __instance.player.factory.GetVegeData(miningID);
				VegeProto vProto = LDB.veges.Select((int)vData.protoId);
				if(vProto == null)
                {
					return;
                }

				timeMined += __instance.miningTick;

				if(timeMined >= vProto.MiningTime * 10000)
                {
					// player did mine the "vegetable" completely, so we need to tell other players
					// NOTE: amount of items picked up is determined here with a RNG seeded with 'vData.id + ((__instance.player.planetData.seed & 16383) << 14)'
					// still do the checks done in PlanetFactory::RemoveVegetationWithComponents() before sending a packet
					if(GameMain.localPlanet.factory.vegePool[vData.id].id != 0)
                    {
						Log.Info($"RemoveVegeWithComponents: {vData.id} | miningID: {miningID}");
						MultiplayerSession.instance.Client.OnVegetationMined(miningID, true, GameMain.localPlanet.id);
                    }
				}
			}
			else if(miningID != 0 && miningType == EObjectType.Vein)
            {
				VeinData vData = __instance.player.factory.GetVeinData(miningID);
				VeinProto vProto = LDB.veins.Select((int)vData.type);
				if(vProto == null)
                {
					return;
                }

				timeMined += __instance.miningTick;

				if(timeMined >= vProto.MiningTime * 10000)
                {
					if(GameMain.localPlanet.factory.veinPool[vData.id].id != 0)
                    {
						Log.Info($"Player mined something from: {miningID}");
						MultiplayerSession.instance.Client.OnVegetationMined(miningID, false, GameMain.localPlanet.id);
                    }
                }
            }
		}
	}
}
