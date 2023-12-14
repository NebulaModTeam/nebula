#region

using System;

#endregion

namespace NebulaWorld.GameStates;

public class GameStatesManager : IDisposable
{
    public static float MaxUPS = 240f;
    public static float MinUPS = 30f;
    public static bool DuringReconnect;
    private static int bufferLength;

    public static long RealGameTick => GameMain.gameTick;
    public static float RealUPS => (float)FPSController.currentUPS;
    public static string ImportedSaveName { get; set; }
    public static GameDesc NewGameDesc { get; set; }
    public static int FragmentSize { get; set; }

    public void Dispose()
    {
        FragmentSize = 0;
    }

    public static void NotifyTickDifference(float delta)
    {
    }

    public static void DoFastReconnect()
    {
        // trigger game exit to main menu
        DuringReconnect = true;
        UIRoot.instance.uiGame.escMenu.OnButton5Click();
    }

    public static void UpdateBufferLength(int length)
    {
        if (length > 0)
        {
            bufferLength = length;
            Multiplayer.Session.World.UpdatePingIndicator(LoadingMessage());
        }
    }

    public static string LoadingMessage()
    {
        var progress = bufferLength * 100f / FragmentSize;
        return $"Downloading {FragmentSize / 1000:n0} KB ({progress:F1}%)";
    }
}
