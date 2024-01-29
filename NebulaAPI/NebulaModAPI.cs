#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using NebulaAPI.GameState;
using NebulaAPI.Interfaces;
// ReSharper disable UnassignedField.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IdentifierTypo

#endregion

namespace NebulaAPI;

[BepInPlugin(API_GUID, API_NAME, ThisAssembly.AssemblyFileVersion)]
[BepInDependency(NEBULA_MODID, BepInDependency.DependencyFlags.SoftDependency)]
[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class NebulaModAPI : BaseUnityPlugin
{
    public const string NEBULA_MODID = "dsp.nebula-multiplayer";

    public const string API_GUID = "dsp.nebula-multiplayer-api";
    public const string API_NAME = "NebulaMultiplayerModApi";

    public const int PLANET_NONE = -2;
    public const int AUTHOR_NONE = -1;
    public const int STAR_NONE = -1;

    private static PropertyInfo multiplayerSessionGetter;
    private static ConstructorInfo binaryWriterConstructor;
    private static ConstructorInfo binaryReaderConstructor;

    public static readonly List<Assembly> TargetAssemblies = [];

    /// <summary>
    ///     Subscribe to receive event when new multiplayer game is started<br />
    ///     (Host sets up a game, or Client establishes connection)
    /// </summary>
    public static Action OnMultiplayerGameStarted;

    /// <summary>
    ///     Subscribe to receive event when multiplayer game end<br />
    ///     (Host ends the game, or Client disconnects)
    /// </summary>
    public static Action OnMultiplayerGameEnded;

    /// <summary>
    ///     Subscribe to receive event when a new star starts loading (client)<br />
    ///     int starIndex - index of star to load<br />
    /// </summary>
    public static Action<int> OnStarLoadRequest;

    /// <summary>
    ///     Subscribe to receive event when a DysonSphere finishes loading (client)<br />
    ///     int starIndex - index of star of dyson sphere to load<br />
    /// </summary>
    public static Action<int> OnDysonSphereLoadFinished;

    /// <summary>
    ///     Subscribe to receive event when a PlanetFactory starts loading (client)<br />
    ///     int planetId - id of planet to load<br />
    /// </summary>
    public static Action<int> OnPlanetLoadRequest;

    /// <summary>
    ///     Subscribe to receive event when a PlanetFactory is finished loading (client)<br />
    ///     int planetId - id of planet to load
    /// </summary>
    public static Action<int> OnPlanetLoadFinished;

    /// <summary>
    ///     Subscribe to receive even when a player joins the game (Host)<br />
    ///     The event fires after the player sync all the data<br />
    ///     <see cref="IPlayerData" /> - joined player data
    /// </summary>
    public static Action<IPlayerData> OnPlayerJoinedGame;

    /// <summary>
    ///     Subscribe to receive even when a player leaves the game (Host)<br />
    ///     The event fires after the player disconnect<br />
    ///     <see cref="IPlayerData" /> - left player data
    /// </summary>
    public static Action<IPlayerData> OnPlayerLeftGame;

    public static bool NebulaIsInstalled { get; set; }

    /// <summary>
    ///     Is this session in multiplayer
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static bool IsMultiplayerActive { get; private set; }

    /// <summary>
    ///     Provides access to MultiplayerSession class
    /// </summary>
    public static IMultiplayerSession MultiplayerSession { get; private set; }

    // internal use
    public static void OnMultiplayerSessionChange(bool isActive)
    {
        IsMultiplayerActive = isActive;
        MultiplayerSession = isActive ? (IMultiplayerSession)multiplayerSessionGetter?.GetValue(null) : null;
    }

    private void Awake()
    {
        NebulaIsInstalled = false;

        foreach (var unused in Chainloader.PluginInfos.Where(pluginInfo => pluginInfo.Value.Metadata.GUID == NEBULA_MODID))
        {
            NebulaIsInstalled = true;
            break;
        }

        if (!NebulaIsInstalled)
        {
            return;
        }

        multiplayerSessionGetter = AccessTools.TypeByName("NebulaWorld.Multiplayer").GetProperty("Session");
        var binaryUtils = AccessTools.TypeByName("NebulaModel.Networking.BinaryUtils");
        binaryWriterConstructor = binaryUtils.GetNestedType("Writer").GetConstructor(Type.EmptyTypes);
        binaryReaderConstructor = binaryUtils.GetNestedType("Reader").GetConstructor([typeof(byte[])]);

        Logger.LogInfo("Nebula API is ready!");
    }

    /// <summary>
    ///     Register all packets within assembly
    /// </summary>
    /// <param name="assembly">Target assembly</param>
    public static void RegisterPackets(Assembly assembly)
    {
        TargetAssemblies.Add(assembly);
    }

    /// <summary>
    ///     Provides access to BinaryWriter with LZ4 compression
    /// </summary>
    public static IWriterProvider GetBinaryWriter()
    {
        if (!NebulaIsInstalled)
        {
            return null;
        }

        return (IWriterProvider)binaryWriterConstructor.Invoke([]);
    }

    /// <summary>
    ///     Provides access to BinaryReader with LZ4 compression
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static IReaderProvider GetBinaryReader(byte[] bytes)
    {
        if (!NebulaIsInstalled)
        {
            return null;
        }

        return (IReaderProvider)binaryReaderConstructor.Invoke([bytes]);
    }
}
