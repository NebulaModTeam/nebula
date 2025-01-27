#region

using System;
using System.ComponentModel;
using BepInEx.Configuration;
using NebulaModel.Attributes;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Utils;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaModel;

[Serializable]
public class MultiplayerOptions : ICloneable
{
    private const string _ngrokAuthtokenDisplayname = "Ngrok Authtoken";

    private bool _streamerMode;

    [DisplayName("Nickname")] public string Nickname { get; set; } = string.Empty;

    [DisplayName("NameTagSize")] public int NameTagSize { get; set; } = 100;

    [DisplayName("Server Password")]
    [Category("Network")]
    [Description("If provided, this will set a password for your hosted server.")]
    [UIContentType(InputField.ContentType.Password)]
    public string ServerPassword { get; set; } = string.Empty;

    public string LastClientPassword { get; set; } = string.Empty;

    [DisplayName("Host Port")]
    [Category("Network")]
    [UIRange(1, ushort.MaxValue)]
    public ushort HostPort { get; set; } = 8469;

    [DisplayName("Enable UPnp/Pmp Support")]
    [Category("Network")]
    [Description(
        "If enabled, attempt to automatically create a port mapping using UPnp/Pmp (only works if your router has this feature and it is enabled)")]
    public bool EnableUPnpOrPmpSupport { get; set; } = false;

    [DisplayName("Enable Experimental Ngrok support")]
    [Category("Network")]
    [Description(
        "If enabled, when hosting a server this will automatically download and install the Ngrok client and set up an Ngrok tunnel that provides an address at which the server can be joined")]
    public bool EnableNgrok { get; set; } = false;

    [DisplayName(_ngrokAuthtokenDisplayname)]
    [Category("Network")]
    [Description("This is required for Ngrok support and can be obtained by creating a free account at https://ngrok.com/")]
    [UICharacterLimit(49)]
    public string NgrokAuthtoken { get; set; } = string.Empty;

    [DisplayName("Ngrok Region")]
    [Category("Network")]
    [Description("Available Regions: us, eu, au, ap, sa, jp, in")]
    public string NgrokRegion { get; set; } = string.Empty;

    [DisplayName("Remember Last IP")]
    [Category("Network")]
    public bool RememberLastIP { get; set; } = true;

    [DisplayName("Remember Last Client Password")]
    [Category("Network")]
    public bool RememberLastClientPassword { get; set; } = true;

    [DisplayName("Enable Discord RPC (requires restart)")]
    [Category("Network")]
    public bool EnableDiscordRPC { get; set; } = true;

    [DisplayName("Auto accept Discord join requests")]
    [Category("Network")]
    public bool AutoAcceptDiscordJoinRequests { get; set; } = false;

    [DisplayName("IP Configuration")]
    [Category("Network")]
    [Description("Configure which type of IP should be used by Discord RPC")]
    public IPUtils.IPConfiguration IPConfiguration { get; set; }

    [DisplayName("Cleanup inactive sessions")]
    [Category("Network")]
    [Description(
        "If disabled the underlying networking library will not cleanup inactive connections. This might solve issues with clients randomly disconnecting and hosts having a 'System.ObjectDisposedException'.")]
    public bool CleanupInactiveSessions { get; set; } = false;

    [DisplayName("Show Lobby Hints")] public bool ShowLobbyHints { get; set; } = true;

    public string LastIP { get; set; } = string.Empty;

    [DisplayName("Sync Ups")]
    [Description("If enabled the UPS of each player is synced. This ensures a similar amount of GameTick() calls.")]
    public bool SyncUps { get; set; } = true;

    [DisplayName("Sync Soil")]
    [Description(
        "If enabled the soil count of each players is added together and used as one big pool for everyone. Note that this is a server side setting applied to all clients.")]
    public bool SyncSoil { get; set; }

    [DisplayName("Streamer mode")]
    [Description(
        "If enabled specific personal information like your IP address is hidden from the ingame chat and input fields.")]
    public bool StreamerMode
    {
        get => _streamerMode;
        set
        {
            _streamerMode = value;

            var ngrokAuthTokenInput = GameObject.Find("list/scroll-view/viewport/content/Network/NgrokAuthtoken")?
                .GetComponentInChildren<InputField>();
            UpdateInputFieldContentType(ref ngrokAuthTokenInput);

            var hostIpInput = GameObject.Find("UI Root/Overlay Canvas/Nebula - Multiplayer Menu/Host IP Address/InputField")?
                .GetComponentInChildren<InputField>();
            UpdateInputFieldContentType(ref hostIpInput);
        }
    }

    [DisplayName("Enable Achievement")]
    [Description("Toggle to enable achievement in multiplayer game")]
    public bool EnableAchievement { get; set; } = true;

    [DisplayName("Enable Other Player Sounds")]
    public bool EnableOtherPlayerSounds { get; set; } = true;

    [DisplayName("Chat Hotkey")]
    [Category("Chat")]
    [Description("Keyboard shortcut to toggle the chat window")]
    public KeyboardShortcut ChatHotkey { get; set; } = new(KeyCode.BackQuote, KeyCode.LeftAlt);

    [DisplayName("Player List Hotkey")]
    [Description("Keyboard shortcut to display the Connected Players Window")]
    public KeyboardShortcut PlayerListHotkey { get; set; } = new(KeyCode.BackQuote);

    [DisplayName("Auto Open Chat")]
    [Category("Chat")]
    [Description("Auto open chat window when receiving message from other players")]
    public bool AutoOpenChat { get; set; } = false;

    [DisplayName("Show Timestamp")]
    [Category("Chat")]
    public bool EnableTimestamp { get; set; } = true;

    [DisplayName("Show system warn message")]
    [Category("Chat")]
    public bool EnableWarnMessage { get; set; } = true;

    [DisplayName("Show system info message")]
    [Category("Chat")]
    public bool EnableInfoMessage { get; set; } = true;

    [DisplayName("Show battle notification message")]
    [Category("Chat")]
    public bool EnableBattleMessage { get; set; } = true;

    [DisplayName("Default chat position")]
    [Category("Chat")]
    public ChatPosition DefaultChatPosition { get; set; } = ChatPosition.LeftMiddle;

    [DisplayName("Default chat size")]
    [Category("Chat")]
    public ChatSize DefaultChatSize { get; set; } = ChatSize.Medium;

    [DisplayName("Notification duration")]
    [Category("Chat")]
    [Description("How long should the active message stay on the screen in seconds")]
    public int NotificationDuration { get; set; } = 15;

    [DisplayName("Chat Window Opacity")]
    [Category("Chat")]
    [UIRange(0f, 1.0f, true)]
    public float ChatWindowOpacity { get; set; } = 0.8f;

    // Detail function group buttons
    public bool PowerGridEnabled { get; set; }
    public bool VeinDistributionEnabled { get; set; }
    public bool SpaceNavigationEnabled { get; set; } = true;
    public bool BuildingWarningEnabled { get; set; } = true;
    public bool BuildingIconEnabled { get; set; } = true;
    public bool GuidingLightEnabled { get; set; } = true;

    public bool RemoteAccessEnabled { get; set; } = false;
    public string RemoteAccessPassword { get; set; } = "";
    public bool AutoPauseEnabled { get; set; } = true;


    public object Clone()
    {
        return MemberwiseClone();
    }

    private void UpdateInputFieldContentType(ref InputField inputField)
    {
        if (inputField == null)
        {
            return;
        }
        inputField.contentType = StreamerMode ? InputField.ContentType.Password : InputField.ContentType.Standard;
        inputField.UpdateLabel();
    }

    public void ModifyInputFieldAtCreation(string displayName, ref InputField inputField)
    {
        switch (displayName)
        {
            case _ngrokAuthtokenDisplayname:
                {
                    UpdateInputFieldContentType(ref inputField);
                    break;
                }
        }
    }
}
