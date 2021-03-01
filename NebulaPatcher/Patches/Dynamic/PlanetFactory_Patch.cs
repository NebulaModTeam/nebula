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
			// If the vegetation hasn't been mined, sent a packet to the other clients
			if (GameMain.localPlanet.factory.vegePool[id].id != 0)
				MultiplayerSession.instance.Client.OnVegetationMined(id, GameMain.localPlanet.id);
			
		}
	}
}
