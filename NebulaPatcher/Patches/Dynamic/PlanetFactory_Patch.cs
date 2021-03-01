using HarmonyLib;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;

namespace NebulaPatcher.Patches.Dynamic
{
	[HarmonyPatch(typeof(PlanetFactory), "RemoveVegeWithComponents")]
	class PlanetFactory_Patch
	{
		public static void Prefix(int id)
		{
			// TODO: Will probably need to revisit this, right now this is called a lot of time without any user interaction.
			// When this occurs the master client should be the one that update us about the state of our veges, entities, veins, etc.
			// So right now, I will limit this to only work if the player is actually mining a vegetable.
			if (GameMain.mainPlayer.controller.actionMine.miningType == EObjectType.Vegetable)
            {
				// If the vegetation hasn't been mined, sent a packet to the other clients
				if (GameMain.localPlanet.factory.vegePool[id].id != 0)
				{
					Log.Info($"RemoveVegeWithComponents: {id}");
					MultiplayerSession.instance.Client.OnVegetationMined(id, GameMain.localPlanet.id);
				}
			}
		}
	}
}
