using System.IO;
using UnityEngine;

namespace NebulaWorld.Statistics
{
    public static class StatisticsManager
    {
        public static bool IsIncommingRequest = false;
        public static bool IsStatisticsNeeded = false;
        public static long[] FakePowerSystemData;

        public static void UpdateTotalChargedEnergy(ref long num2, int targetIndex)
        {
            num2 = 0L;
            //Total Stored Energy for "Entire Star Cluster"
            if (targetIndex == -1)
            {
                //For the host and singleplayer, use normal calculation. For the clients, use Data from the server
                if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
                {
                    for (int i = 0; i < FakePowerSystemData.Length; i++)
                    {
                        num2 += FakePowerSystemData[i];
                    }
                }
                else
                {
                    for (int i = 0; i < GameMain.data.factoryCount; i++)
                    {
                        PowerSystem powerSystem = GameMain.data.factories[i].powerSystem;
                        int netCursor = powerSystem.netCursor;
                        PowerNetwork[] netPool = powerSystem.netPool;
                        for (int j = 1; j < netCursor; j++)
                        {
                            num2 += netPool[j].energyStored;
                        }
                    }
                }
            }
            //Total Stored Energy for "Local Planet"
            else if (targetIndex == 0)
            {
                if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
                {
                    num2 = GameMain.data.localPlanet.factoryIndex != -1 ? FakePowerSystemData[GameMain.data.localPlanet.factoryIndex] : 0;
                }
                else
                {
                    PowerSystem powerSystem2 = GameMain.data.localPlanet?.factory?.powerSystem;
                    int netCursor2 = powerSystem2.netCursor;
                    PowerNetwork[] netPool2 = powerSystem2.netPool;
                    for (int l = 1; l < netCursor2; l++)
                    {
                        num2 += netPool2[l].energyStored;
                    }
                }
            }   
            //Total Stored Energy for "Picking specific planet"
            else if (targetIndex % 100 > 0)
            {
                if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
                {
                    for (int i = 0; i < GameMain.data.factoryCount; i++)
                    {
                        if (targetIndex == GameMain.data.factories[i].planetId)
                        {
                            num2 = FakePowerSystemData[i];
                            break;
                        }
                    }
                }
                else
                {
                    PlanetData planetData = GameMain.data.galaxy.PlanetById(targetIndex);
                    PowerSystem powerSystem3 = planetData.factory.powerSystem;
                    int netCursor3 = powerSystem3.netCursor;
                    PowerNetwork[] netPool3 = powerSystem3.netPool;
                    for (int m = 1; m < netCursor3; m++)
                    {
                        num2 += netPool3[m].energyStored;
                    }
                    Debug.Log(num2);
                }
            }
            //Total Stored Energy for "Picking Star System"
            else if (targetIndex % 100 == 0)
            {
                int starId = targetIndex / 100;
                StarData starData = GameMain.data.galaxy.StarById(starId);
                for (int n = 0; n < starData.planetCount; n++)
                {
                    if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
                    {
                        if (starData.planets[n].factoryIndex != -1)
                        {
                            num2 += FakePowerSystemData[starData.planets[n].factoryIndex];
                        }
                    }
                    else if (starData.planets[n].factory != null)
                    {
                        PowerSystem powerSystem4 = starData.planets[n].factory.powerSystem;
                        int netCursor4 = powerSystem4.netCursor;
                        PowerNetwork[] netPool4 = powerSystem4.netPool;
                        for (int num9 = 1; num9 < netCursor4; num9++)
                        {
                            num2 += netPool4[num9].energyStored;
                        }
                    }
                }
            }
        }

        public static void ImporAllHistorytData(BinaryReader br)
        {
            GameStatData Stats = GameMain.statistics;

            //Import Factory statistics
            int factoryCount = br.ReadInt32();

                for (int i = 0; i < factoryCount; i++)
            {
                if (Stats.production.factoryStatPool[i] == null)
                {
                    Stats.production.factoryStatPool[i] = new FactoryProductionStat();
                    Stats.production.factoryStatPool[i].Init();
                }
                Stats.production.factoryStatPool[i].Import(br);
            }

            //Import Reserach Statistics
            int num = br.ReadInt32();
            if (num > Stats.techHashedHistory.Length)
            {
                Stats.techHashedHistory = new int[num];
            }
            for (int i = 0; i < num; i++)
            {
                Stats.techHashedHistory[i] = br.ReadInt32();
            }

            //Resfresh the view
            UIRoot.instance.uiGame.production.ComputeDisplayEntries();
        }
    }
}
