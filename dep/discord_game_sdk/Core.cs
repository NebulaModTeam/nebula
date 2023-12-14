#region

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Discord
{
    public enum Result
    {
        Ok = 0,
        ServiceUnavailable = 1,
        InvalidVersion = 2,
        LockFailed = 3,
        InternalError = 4,
        InvalidPayload = 5,
        InvalidCommand = 6,
        InvalidPermissions = 7,
        NotFetched = 8,
        NotFound = 9,
        Conflict = 10,
        InvalidSecret = 11,
        InvalidJoinSecret = 12,
        NoEligibleActivity = 13,
        InvalidInvite = 14,
        NotAuthenticated = 15,
        InvalidAccessToken = 16,
        ApplicationMismatch = 17,
        InvalidDataUrl = 18,
        InvalidBase64 = 19,
        NotFiltered = 20,
        LobbyFull = 21,
        InvalidLobbySecret = 22,
        InvalidFilename = 23,
        InvalidFileSize = 24,
        InvalidEntitlement = 25,
        NotInstalled = 26,
        NotRunning = 27,
        InsufficientBuffer = 28,
        PurchaseCanceled = 29,
        InvalidGuild = 30,
        InvalidEvent = 31,
        InvalidChannel = 32,
        InvalidOrigin = 33,
        RateLimited = 34,
        OAuth2Error = 35,
        SelectChannelTimeout = 36,
        GetGuildTimeout = 37,
        SelectVoiceForceRequired = 38,
        CaptureShortcutAlreadyListening = 39,
        UnauthorizedForAchievement = 40,
        InvalidGiftCode = 41,
        PurchaseError = 42,
        TransactionAborted = 43
    }

    public enum CreateFlags
    {
        Default = 0,
        NoRequireDiscord = 1
    }

    public enum LogLevel
    {
        Error = 1,
        Warn,
        Info,
        Debug
    }

    public enum UserFlag
    {
        Partner = 2,
        HypeSquadEvents = 4,
        HypeSquadHouse1 = 64,
        HypeSquadHouse2 = 128,
        HypeSquadHouse3 = 256
    }

    public enum PremiumType
    {
        None = 0,
        Tier1 = 1,
        Tier2 = 2
    }

    public enum ImageType
    {
        User
    }

    public enum ActivityType
    {
        Playing,
        Streaming,
        Listening,
        Watching
    }

    public enum ActivityActionType
    {
        Join = 1,
        Spectate
    }

    public enum ActivityJoinRequestReply
    {
        No,
        Yes,
        Ignore
    }

    public enum Status
    {
        Offline = 0,
        Online = 1,
        Idle = 2,
        DoNotDisturb = 3
    }

    public enum RelationshipType
    {
        None,
        Friend,
        Blocked,
        PendingIncoming,
        PendingOutgoing,
        Implicit
    }

    public enum LobbyType
    {
        Private = 1,
        Public
    }

    public enum LobbySearchComparison
    {
        LessThanOrEqual = -2,
        LessThan,
        Equal,
        GreaterThan,
        GreaterThanOrEqual,
        NotEqual
    }

    public enum LobbySearchCast
    {
        String = 1,
        Number
    }

    public enum LobbySearchDistance
    {
        Local,
        Default,
        Extended,
        Global
    }

    public enum EntitlementType
    {
        Purchase = 1,
        PremiumSubscription,
        DeveloperGift,
        TestModePurchase,
        FreePurchase,
        UserGift,
        PremiumPurchase
    }

    public enum SkuType
    {
        Application = 1,
        DLC,
        Consumable,
        Bundle
    }

    public enum InputModeType
    {
        VoiceActivity = 0,
        PushToTalk
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct User
    {
        public Int64 Id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Username;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string Discriminator;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Avatar;

        public bool Bot;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct OAuth2Token
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string AccessToken;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string Scopes;

        public Int64 Expires;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public partial struct ImageHandle
    {
        public ImageType Type;

        public Int64 Id;

        public UInt32 Size;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ImageDimensions
    {
        public UInt32 Width;

        public UInt32 Height;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ActivityTimestamps
    {
        public Int64 Start;

        public Int64 End;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ActivityAssets
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string LargeImage;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string LargeText;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string SmallImage;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string SmallText;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct PartySize
    {
        public Int32 CurrentSize;

        public Int32 MaxSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ActivityParty
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Id;

        public PartySize Size;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ActivitySecrets
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Match;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Join;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Spectate;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Activity
    {
        public ActivityType Type;

        public Int64 ApplicationId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string State;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Details;

        public ActivityTimestamps Timestamps;

        public ActivityAssets Assets;

        public ActivityParty Party;

        public ActivitySecrets Secrets;

        public bool Instance;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Presence
    {
        public Status Status;

        public Activity Activity;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Relationship
    {
        public RelationshipType Type;

        public User User;

        public Presence Presence;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Lobby
    {
        public Int64 Id;

        public LobbyType Type;

        public Int64 OwnerId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Secret;

        public UInt32 Capacity;

        public bool Locked;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FileStat
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string Filename;

        public UInt64 Size;

        public UInt64 LastModified;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Entitlement
    {
        public Int64 Id;

        public EntitlementType Type;

        public Int64 SkuId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct SkuPrice
    {
        public UInt32 Amount;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Currency;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Sku
    {
        public Int64 Id;

        public SkuType Type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Name;

        public SkuPrice Price;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct InputMode
    {
        public InputModeType Type;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Shortcut;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct UserAchievement
    {
        public Int64 UserId;

        public Int64 AchievementId;

        public byte PercentComplete;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string UnlockedAt;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public partial struct LobbyTransaction
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetTypeMethod(IntPtr methodsPtr, LobbyType type);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetOwnerMethod(IntPtr methodsPtr, long ownerId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetCapacityMethod(IntPtr methodsPtr, uint capacity);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key,
                [MarshalAs(UnmanagedType.LPStr)] string value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetLockedMethod(IntPtr methodsPtr, bool locked);

            internal SetTypeMethod SetType;

            internal SetOwnerMethod SetOwner;

            internal SetCapacityMethod SetCapacity;

            internal SetMetadataMethod SetMetadata;

            internal DeleteMetadataMethod DeleteMetadata;

            internal SetLockedMethod SetLocked;
        }

        internal IntPtr MethodsPtr;

        internal Object MethodsStructure;

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public void SetType(LobbyType type)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetType(MethodsPtr, type);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void SetOwner(long ownerId)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetOwner(MethodsPtr, ownerId);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void SetCapacity(uint capacity)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetCapacity(MethodsPtr, capacity);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void SetMetadata(string key, string value)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetMetadata(MethodsPtr, key, value);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void DeleteMetadata(string key)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.DeleteMetadata(MethodsPtr, key);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void SetLocked(bool locked)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetLocked(MethodsPtr, locked);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public partial struct LobbyMemberTransaction
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key,
                [MarshalAs(UnmanagedType.LPStr)] string value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

            internal SetMetadataMethod SetMetadata;

            internal DeleteMetadataMethod DeleteMetadata;
        }

        internal IntPtr MethodsPtr;

        internal Object MethodsStructure;

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public void SetMetadata(string key, string value)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.SetMetadata(MethodsPtr, key, value);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void DeleteMetadata(string key)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.DeleteMetadata(MethodsPtr, key);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public partial struct LobbySearchQuery
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result FilterMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key,
                LobbySearchComparison comparison, LobbySearchCast cast, [MarshalAs(UnmanagedType.LPStr)] string value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SortMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key,
                LobbySearchCast cast, [MarshalAs(UnmanagedType.LPStr)] string value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result LimitMethod(IntPtr methodsPtr, uint limit);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result DistanceMethod(IntPtr methodsPtr, LobbySearchDistance distance);

            internal FilterMethod Filter;

            internal SortMethod Sort;

            internal LimitMethod Limit;

            internal DistanceMethod Distance;
        }

        internal IntPtr MethodsPtr;

        internal Object MethodsStructure;

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public void Filter(string key, LobbySearchComparison comparison, LobbySearchCast cast, string value)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.Filter(MethodsPtr, key, comparison, cast, value);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void Sort(string key, LobbySearchCast cast, string value)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.Sort(MethodsPtr, key, cast, value);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void Limit(uint limit)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.Limit(MethodsPtr, limit);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }

        public void Distance(LobbySearchDistance distance)
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                var res = Methods.Distance(MethodsPtr, distance);
                if (res != Result.Ok)
                {
                    throw new ResultException(res);
                }
            }
        }
    }

    public class ResultException : Exception
    {
        public readonly Result Result;

        public ResultException(Result result) : base(result.ToString())
        {
        }
    }

    public partial class Discord : IDisposable
    {
        public delegate void SetLogHookHandler(LogLevel level, string message);

        private AchievementManager.FFIEvents AchievementEvents;

        private readonly IntPtr AchievementEventsPtr;

        internal AchievementManager AchievementManagerInstance;

        private ActivityManager.FFIEvents ActivityEvents;

        private readonly IntPtr ActivityEventsPtr;

        internal ActivityManager ActivityManagerInstance;

        private ApplicationManager.FFIEvents ApplicationEvents;

        private readonly IntPtr ApplicationEventsPtr;

        internal ApplicationManager ApplicationManagerInstance;

        private readonly FFIEvents Events;

        private readonly IntPtr EventsPtr;

        private ImageManager.FFIEvents ImageEvents;

        private readonly IntPtr ImageEventsPtr;

        internal ImageManager ImageManagerInstance;

        private LobbyManager.FFIEvents LobbyEvents;

        private readonly IntPtr LobbyEventsPtr;

        internal LobbyManager LobbyManagerInstance;

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        private NetworkManager.FFIEvents NetworkEvents;

        private readonly IntPtr NetworkEventsPtr;

        internal NetworkManager NetworkManagerInstance;

        private OverlayManager.FFIEvents OverlayEvents;

        private readonly IntPtr OverlayEventsPtr;

        internal OverlayManager OverlayManagerInstance;

        private RelationshipManager.FFIEvents RelationshipEvents;

        private readonly IntPtr RelationshipEventsPtr;

        internal RelationshipManager RelationshipManagerInstance;

        private GCHandle SelfHandle;

        private GCHandle? setLogHook;

        private StorageManager.FFIEvents StorageEvents;

        private readonly IntPtr StorageEventsPtr;

        internal StorageManager StorageManagerInstance;

        private StoreManager.FFIEvents StoreEvents;

        private readonly IntPtr StoreEventsPtr;

        internal StoreManager StoreManagerInstance;

        private UserManager.FFIEvents UserEvents;

        private readonly IntPtr UserEventsPtr;

        internal UserManager UserManagerInstance;

        private VoiceManager.FFIEvents VoiceEvents;

        private readonly IntPtr VoiceEventsPtr;

        internal VoiceManager VoiceManagerInstance;

        public Discord(long clientId, ulong flags)
        {
            FFICreateParams createParams;
            createParams.ClientId = clientId;
            createParams.Flags = flags;
            Events = new FFIEvents();
            EventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(Events));
            createParams.Events = EventsPtr;
            SelfHandle = GCHandle.Alloc(this);
            createParams.EventData = GCHandle.ToIntPtr(SelfHandle);
            ApplicationEvents = new ApplicationManager.FFIEvents();
            ApplicationEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ApplicationEvents));
            createParams.ApplicationEvents = ApplicationEventsPtr;
            createParams.ApplicationVersion = 1;
            UserEvents = new UserManager.FFIEvents();
            UserEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(UserEvents));
            createParams.UserEvents = UserEventsPtr;
            createParams.UserVersion = 1;
            ImageEvents = new ImageManager.FFIEvents();
            ImageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ImageEvents));
            createParams.ImageEvents = ImageEventsPtr;
            createParams.ImageVersion = 1;
            ActivityEvents = new ActivityManager.FFIEvents();
            ActivityEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ActivityEvents));
            createParams.ActivityEvents = ActivityEventsPtr;
            createParams.ActivityVersion = 1;
            RelationshipEvents = new RelationshipManager.FFIEvents();
            RelationshipEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(RelationshipEvents));
            createParams.RelationshipEvents = RelationshipEventsPtr;
            createParams.RelationshipVersion = 1;
            LobbyEvents = new LobbyManager.FFIEvents();
            LobbyEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(LobbyEvents));
            createParams.LobbyEvents = LobbyEventsPtr;
            createParams.LobbyVersion = 1;
            NetworkEvents = new NetworkManager.FFIEvents();
            NetworkEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(NetworkEvents));
            createParams.NetworkEvents = NetworkEventsPtr;
            createParams.NetworkVersion = 1;
            OverlayEvents = new OverlayManager.FFIEvents();
            OverlayEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(OverlayEvents));
            createParams.OverlayEvents = OverlayEventsPtr;
            createParams.OverlayVersion = 1;
            StorageEvents = new StorageManager.FFIEvents();
            StorageEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(StorageEvents));
            createParams.StorageEvents = StorageEventsPtr;
            createParams.StorageVersion = 1;
            StoreEvents = new StoreManager.FFIEvents();
            StoreEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(StoreEvents));
            createParams.StoreEvents = StoreEventsPtr;
            createParams.StoreVersion = 1;
            VoiceEvents = new VoiceManager.FFIEvents();
            VoiceEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(VoiceEvents));
            createParams.VoiceEvents = VoiceEventsPtr;
            createParams.VoiceVersion = 1;
            AchievementEvents = new AchievementManager.FFIEvents();
            AchievementEventsPtr = Marshal.AllocHGlobal(Marshal.SizeOf(AchievementEvents));
            createParams.AchievementEvents = AchievementEventsPtr;
            createParams.AchievementVersion = 1;
            InitEvents(EventsPtr, ref Events);
            var result = DiscordCreate(2, ref createParams, out MethodsPtr);
            if (result != Result.Ok)
            {
                Dispose();
                throw new ResultException(result);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public void Dispose()
        {
            if (MethodsPtr != IntPtr.Zero)
            {
                Methods.Destroy(MethodsPtr);
            }
            SelfHandle.Free();
            Marshal.FreeHGlobal(EventsPtr);
            Marshal.FreeHGlobal(ApplicationEventsPtr);
            Marshal.FreeHGlobal(UserEventsPtr);
            Marshal.FreeHGlobal(ImageEventsPtr);
            Marshal.FreeHGlobal(ActivityEventsPtr);
            Marshal.FreeHGlobal(RelationshipEventsPtr);
            Marshal.FreeHGlobal(LobbyEventsPtr);
            Marshal.FreeHGlobal(NetworkEventsPtr);
            Marshal.FreeHGlobal(OverlayEventsPtr);
            Marshal.FreeHGlobal(StorageEventsPtr);
            Marshal.FreeHGlobal(StoreEventsPtr);
            Marshal.FreeHGlobal(VoiceEventsPtr);
            Marshal.FreeHGlobal(AchievementEventsPtr);
            if (setLogHook.HasValue)
            {
                setLogHook.Value.Free();
            }
        }

        [DllImport(Constants.DllName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result DiscordCreate(uint version, ref FFICreateParams createParams, out IntPtr manager);

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public void RunCallbacks()
        {
            var res = Methods.RunCallbacks(MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void SetLogHookCallbackImpl(IntPtr ptr, LogLevel level, string message)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SetLogHookHandler)h.Target;
            callback(level, message);
        }

        public void SetLogHook(LogLevel minLevel, SetLogHookHandler callback)
        {
            if (setLogHook.HasValue)
            {
                setLogHook.Value.Free();
            }
            setLogHook = GCHandle.Alloc(callback);
            Methods.SetLogHook(MethodsPtr, minLevel, GCHandle.ToIntPtr(setLogHook.Value), SetLogHookCallbackImpl);
        }

        public ApplicationManager GetApplicationManager()
        {
            if (ApplicationManagerInstance == null)
            {
                ApplicationManagerInstance = new ApplicationManager(
                    Methods.GetApplicationManager(MethodsPtr),
                    ApplicationEventsPtr,
                    ref ApplicationEvents
                );
            }
            return ApplicationManagerInstance;
        }

        public UserManager GetUserManager()
        {
            if (UserManagerInstance == null)
            {
                UserManagerInstance = new UserManager(
                    Methods.GetUserManager(MethodsPtr),
                    UserEventsPtr,
                    ref UserEvents
                );
            }
            return UserManagerInstance;
        }

        public ImageManager GetImageManager()
        {
            if (ImageManagerInstance == null)
            {
                ImageManagerInstance = new ImageManager(
                    Methods.GetImageManager(MethodsPtr),
                    ImageEventsPtr,
                    ref ImageEvents
                );
            }
            return ImageManagerInstance;
        }

        public ActivityManager GetActivityManager()
        {
            if (ActivityManagerInstance == null)
            {
                ActivityManagerInstance = new ActivityManager(
                    Methods.GetActivityManager(MethodsPtr),
                    ActivityEventsPtr,
                    ref ActivityEvents
                );
            }
            return ActivityManagerInstance;
        }

        public RelationshipManager GetRelationshipManager()
        {
            if (RelationshipManagerInstance == null)
            {
                RelationshipManagerInstance = new RelationshipManager(
                    Methods.GetRelationshipManager(MethodsPtr),
                    RelationshipEventsPtr,
                    ref RelationshipEvents
                );
            }
            return RelationshipManagerInstance;
        }

        public LobbyManager GetLobbyManager()
        {
            if (LobbyManagerInstance == null)
            {
                LobbyManagerInstance = new LobbyManager(
                    Methods.GetLobbyManager(MethodsPtr),
                    LobbyEventsPtr,
                    ref LobbyEvents
                );
            }
            return LobbyManagerInstance;
        }

        public NetworkManager GetNetworkManager()
        {
            if (NetworkManagerInstance == null)
            {
                NetworkManagerInstance = new NetworkManager(
                    Methods.GetNetworkManager(MethodsPtr),
                    NetworkEventsPtr,
                    ref NetworkEvents
                );
            }
            return NetworkManagerInstance;
        }

        public OverlayManager GetOverlayManager()
        {
            if (OverlayManagerInstance == null)
            {
                OverlayManagerInstance = new OverlayManager(
                    Methods.GetOverlayManager(MethodsPtr),
                    OverlayEventsPtr,
                    ref OverlayEvents
                );
            }
            return OverlayManagerInstance;
        }

        public StorageManager GetStorageManager()
        {
            if (StorageManagerInstance == null)
            {
                StorageManagerInstance = new StorageManager(
                    Methods.GetStorageManager(MethodsPtr),
                    StorageEventsPtr,
                    ref StorageEvents
                );
            }
            return StorageManagerInstance;
        }

        public StoreManager GetStoreManager()
        {
            if (StoreManagerInstance == null)
            {
                StoreManagerInstance = new StoreManager(
                    Methods.GetStoreManager(MethodsPtr),
                    StoreEventsPtr,
                    ref StoreEvents
                );
            }
            return StoreManagerInstance;
        }

        public VoiceManager GetVoiceManager()
        {
            if (VoiceManagerInstance == null)
            {
                VoiceManagerInstance = new VoiceManager(
                    Methods.GetVoiceManager(MethodsPtr),
                    VoiceEventsPtr,
                    ref VoiceEvents
                );
            }
            return VoiceManagerInstance;
        }

        public AchievementManager GetAchievementManager()
        {
            if (AchievementManagerInstance == null)
            {
                AchievementManagerInstance = new AchievementManager(
                    Methods.GetAchievementManager(MethodsPtr),
                    AchievementEventsPtr,
                    ref AchievementEvents
                );
            }
            return AchievementManagerInstance;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DestroyHandler(IntPtr MethodsPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result RunCallbacksMethod(IntPtr methodsPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetLogHookCallback(IntPtr ptr, LogLevel level,
                [MarshalAs(UnmanagedType.LPStr)] string message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetLogHookMethod(IntPtr methodsPtr, LogLevel minLevel, IntPtr callbackData,
                SetLogHookCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetApplicationManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetUserManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetImageManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetActivityManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetRelationshipManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetLobbyManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetNetworkManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetOverlayManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetStorageManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetStoreManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetVoiceManagerMethod(IntPtr discordPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate IntPtr GetAchievementManagerMethod(IntPtr discordPtr);

            internal DestroyHandler Destroy;

            internal RunCallbacksMethod RunCallbacks;

            internal SetLogHookMethod SetLogHook;

            internal GetApplicationManagerMethod GetApplicationManager;

            internal GetUserManagerMethod GetUserManager;

            internal GetImageManagerMethod GetImageManager;

            internal GetActivityManagerMethod GetActivityManager;

            internal GetRelationshipManagerMethod GetRelationshipManager;

            internal GetLobbyManagerMethod GetLobbyManager;

            internal GetNetworkManagerMethod GetNetworkManager;

            internal GetOverlayManagerMethod GetOverlayManager;

            internal GetStorageManagerMethod GetStorageManager;

            internal GetStoreManagerMethod GetStoreManager;

            internal GetVoiceManagerMethod GetVoiceManager;

            internal GetAchievementManagerMethod GetAchievementManager;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFICreateParams
        {
            internal Int64 ClientId;

            internal UInt64 Flags;

            internal IntPtr Events;

            internal IntPtr EventData;

            internal IntPtr ApplicationEvents;

            internal UInt32 ApplicationVersion;

            internal IntPtr UserEvents;

            internal UInt32 UserVersion;

            internal IntPtr ImageEvents;

            internal UInt32 ImageVersion;

            internal IntPtr ActivityEvents;

            internal UInt32 ActivityVersion;

            internal IntPtr RelationshipEvents;

            internal UInt32 RelationshipVersion;

            internal IntPtr LobbyEvents;

            internal UInt32 LobbyVersion;

            internal IntPtr NetworkEvents;

            internal UInt32 NetworkVersion;

            internal IntPtr OverlayEvents;

            internal UInt32 OverlayVersion;

            internal IntPtr StorageEvents;

            internal UInt32 StorageVersion;

            internal IntPtr StoreEvents;

            internal UInt32 StoreVersion;

            internal IntPtr VoiceEvents;

            internal UInt32 VoiceVersion;

            internal IntPtr AchievementEvents;

            internal UInt32 AchievementVersion;
        }
    }

    internal class MonoPInvokeCallbackAttribute : Attribute
    {
    }

    public partial class ApplicationManager
    {
        public delegate void GetOAuth2TokenHandler(Result result, ref OAuth2Token oauth2Token);

        public delegate void GetTicketHandler(Result result, ref string data);

        public delegate void ValidateOrExitHandler(Result result);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal ApplicationManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        [MonoPInvokeCallback]
        private static void ValidateOrExitCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ValidateOrExitHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void ValidateOrExit(ValidateOrExitHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ValidateOrExit(MethodsPtr, GCHandle.ToIntPtr(wrapped), ValidateOrExitCallbackImpl);
        }

        public string GetCurrentLocale()
        {
            var ret = new StringBuilder(128);
            Methods.GetCurrentLocale(MethodsPtr, ret);
            return ret.ToString();
        }

        public string GetCurrentBranch()
        {
            var ret = new StringBuilder(4096);
            Methods.GetCurrentBranch(MethodsPtr, ret);
            return ret.ToString();
        }

        [MonoPInvokeCallback]
        private static void GetOAuth2TokenCallbackImpl(IntPtr ptr, Result result, ref OAuth2Token oauth2Token)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (GetOAuth2TokenHandler)h.Target;
            h.Free();
            callback(result, ref oauth2Token);
        }

        public void GetOAuth2Token(GetOAuth2TokenHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.GetOAuth2Token(MethodsPtr, GCHandle.ToIntPtr(wrapped), GetOAuth2TokenCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void GetTicketCallbackImpl(IntPtr ptr, Result result, ref string data)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (GetTicketHandler)h.Target;
            h.Free();
            callback(result, ref data);
        }

        public void GetTicket(GetTicketHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.GetTicket(MethodsPtr, GCHandle.ToIntPtr(wrapped), GetTicketCallbackImpl);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ValidateOrExitCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ValidateOrExitMethod(IntPtr methodsPtr, IntPtr callbackData,
                ValidateOrExitCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetCurrentLocaleMethod(IntPtr methodsPtr, StringBuilder locale);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetCurrentBranchMethod(IntPtr methodsPtr, StringBuilder branch);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetOAuth2TokenCallback(IntPtr ptr, Result result, ref OAuth2Token oauth2Token);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetOAuth2TokenMethod(IntPtr methodsPtr, IntPtr callbackData,
                GetOAuth2TokenCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetTicketCallback(IntPtr ptr, Result result,
                [MarshalAs(UnmanagedType.LPStr)] ref string data);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetTicketMethod(IntPtr methodsPtr, IntPtr callbackData, GetTicketCallback callback);

            internal ValidateOrExitMethod ValidateOrExit;

            internal GetCurrentLocaleMethod GetCurrentLocale;

            internal GetCurrentBranchMethod GetCurrentBranch;

            internal GetOAuth2TokenMethod GetOAuth2Token;

            internal GetTicketMethod GetTicket;
        }
    }

    public partial class UserManager
    {
        public delegate void CurrentUserUpdateHandler();

        public delegate void GetUserHandler(Result result, ref User user);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal UserManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event CurrentUserUpdateHandler OnCurrentUserUpdate;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnCurrentUserUpdate = OnCurrentUserUpdateImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public User GetCurrentUser()
        {
            var ret = new User();
            var res = Methods.GetCurrentUser(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void GetUserCallbackImpl(IntPtr ptr, Result result, ref User user)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (GetUserHandler)h.Target;
            h.Free();
            callback(result, ref user);
        }

        public void GetUser(long userId, GetUserHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.GetUser(MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), GetUserCallbackImpl);
        }

        public PremiumType GetCurrentUserPremiumType()
        {
            var ret = new PremiumType();
            var res = Methods.GetCurrentUserPremiumType(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public bool CurrentUserHasFlag(UserFlag flag)
        {
            var ret = new bool();
            var res = Methods.CurrentUserHasFlag(MethodsPtr, flag, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void OnCurrentUserUpdateImpl(IntPtr ptr)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.UserManagerInstance.OnCurrentUserUpdate != null)
            {
                d.UserManagerInstance.OnCurrentUserUpdate.Invoke();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CurrentUserUpdateHandler(IntPtr ptr);

            internal CurrentUserUpdateHandler OnCurrentUserUpdate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetCurrentUserMethod(IntPtr methodsPtr, ref User currentUser);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetUserCallback(IntPtr ptr, Result result, ref User user);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetUserMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, GetUserCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetCurrentUserPremiumTypeMethod(IntPtr methodsPtr, ref PremiumType premiumType);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result CurrentUserHasFlagMethod(IntPtr methodsPtr, UserFlag flag, ref bool hasFlag);

            internal GetCurrentUserMethod GetCurrentUser;

            internal GetUserMethod GetUser;

            internal GetCurrentUserPremiumTypeMethod GetCurrentUserPremiumType;

            internal CurrentUserHasFlagMethod CurrentUserHasFlag;
        }
    }

    public partial class ImageManager
    {
        public delegate void FetchHandler(Result result, ImageHandle handleResult);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal ImageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        [MonoPInvokeCallback]
        private static void FetchCallbackImpl(IntPtr ptr, Result result, ImageHandle handleResult)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (FetchHandler)h.Target;
            h.Free();
            callback(result, handleResult);
        }

        public void Fetch(ImageHandle handle, bool refresh, FetchHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.Fetch(MethodsPtr, handle, refresh, GCHandle.ToIntPtr(wrapped), FetchCallbackImpl);
        }

        public ImageDimensions GetDimensions(ImageHandle handle)
        {
            var ret = new ImageDimensions();
            var res = Methods.GetDimensions(MethodsPtr, handle, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public void GetData(ImageHandle handle, byte[] data)
        {
            var res = Methods.GetData(MethodsPtr, handle, data, data.Length);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchCallback(IntPtr ptr, Result result, ImageHandle handleResult);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchMethod(IntPtr methodsPtr, ImageHandle handle, bool refresh, IntPtr callbackData,
                FetchCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetDimensionsMethod(IntPtr methodsPtr, ImageHandle handle, ref ImageDimensions dimensions);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetDataMethod(IntPtr methodsPtr, ImageHandle handle, byte[] data, int dataLen);

            internal FetchMethod Fetch;

            internal GetDimensionsMethod GetDimensions;

            internal GetDataMethod GetData;
        }
    }

    public partial class ActivityManager
    {
        public delegate void AcceptInviteHandler(Result result);

        public delegate void ActivityInviteHandler(ActivityActionType type, ref User user, ref Activity activity);

        public delegate void ActivityJoinHandler(string secret);

        public delegate void ActivityJoinRequestHandler(ref User user);

        public delegate void ActivitySpectateHandler(string secret);

        public delegate void ClearActivityHandler(Result result);

        public delegate void SendInviteHandler(Result result);

        public delegate void SendRequestReplyHandler(Result result);

        public delegate void UpdateActivityHandler(Result result);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal ActivityManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event ActivityJoinHandler OnActivityJoin;

        public event ActivitySpectateHandler OnActivitySpectate;

        public event ActivityJoinRequestHandler OnActivityJoinRequest;

        public event ActivityInviteHandler OnActivityInvite;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnActivityJoin = OnActivityJoinImpl;
            events.OnActivitySpectate = OnActivitySpectateImpl;
            events.OnActivityJoinRequest = OnActivityJoinRequestImpl;
            events.OnActivityInvite = OnActivityInviteImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public void RegisterCommand(string command)
        {
            var res = Methods.RegisterCommand(MethodsPtr, command);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public void RegisterSteam(uint steamId)
        {
            var res = Methods.RegisterSteam(MethodsPtr, steamId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void UpdateActivityCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (UpdateActivityHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void UpdateActivity(Activity activity, UpdateActivityHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.UpdateActivity(MethodsPtr, ref activity, GCHandle.ToIntPtr(wrapped), UpdateActivityCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void ClearActivityCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ClearActivityHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void ClearActivity(ClearActivityHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ClearActivity(MethodsPtr, GCHandle.ToIntPtr(wrapped), ClearActivityCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void SendRequestReplyCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SendRequestReplyHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SendRequestReply(long userId, ActivityJoinRequestReply reply, SendRequestReplyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SendRequestReply(MethodsPtr, userId, reply, GCHandle.ToIntPtr(wrapped), SendRequestReplyCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void SendInviteCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SendInviteHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SendInvite(long userId, ActivityActionType type, string content, SendInviteHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SendInvite(MethodsPtr, userId, type, content, GCHandle.ToIntPtr(wrapped), SendInviteCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void AcceptInviteCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (AcceptInviteHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void AcceptInvite(long userId, AcceptInviteHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.AcceptInvite(MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), AcceptInviteCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OnActivityJoinImpl(IntPtr ptr, string secret)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.ActivityManagerInstance.OnActivityJoin != null)
            {
                d.ActivityManagerInstance.OnActivityJoin.Invoke(secret);
            }
        }

        [MonoPInvokeCallback]
        private static void OnActivitySpectateImpl(IntPtr ptr, string secret)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.ActivityManagerInstance.OnActivitySpectate != null)
            {
                d.ActivityManagerInstance.OnActivitySpectate.Invoke(secret);
            }
        }

        [MonoPInvokeCallback]
        private static void OnActivityJoinRequestImpl(IntPtr ptr, ref User user)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.ActivityManagerInstance.OnActivityJoinRequest != null)
            {
                d.ActivityManagerInstance.OnActivityJoinRequest.Invoke(ref user);
            }
        }

        [MonoPInvokeCallback]
        private static void OnActivityInviteImpl(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.ActivityManagerInstance.OnActivityInvite != null)
            {
                d.ActivityManagerInstance.OnActivityInvite.Invoke(type, ref user, ref activity);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ActivityJoinHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ActivitySpectateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ActivityJoinRequestHandler(IntPtr ptr, ref User user);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ActivityInviteHandler(IntPtr ptr, ActivityActionType type, ref User user,
                ref Activity activity);

            internal ActivityJoinHandler OnActivityJoin;

            internal ActivitySpectateHandler OnActivitySpectate;

            internal ActivityJoinRequestHandler OnActivityJoinRequest;

            internal ActivityInviteHandler OnActivityInvite;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result RegisterCommandMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result RegisterSteamMethod(IntPtr methodsPtr, uint steamId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateActivityCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateActivityMethod(IntPtr methodsPtr, ref Activity activity, IntPtr callbackData,
                UpdateActivityCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ClearActivityCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ClearActivityMethod(IntPtr methodsPtr, IntPtr callbackData, ClearActivityCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendRequestReplyCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendRequestReplyMethod(IntPtr methodsPtr, long userId, ActivityJoinRequestReply reply,
                IntPtr callbackData, SendRequestReplyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendInviteCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendInviteMethod(IntPtr methodsPtr, long userId, ActivityActionType type,
                [MarshalAs(UnmanagedType.LPStr)] string content, IntPtr callbackData, SendInviteCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void AcceptInviteCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void AcceptInviteMethod(IntPtr methodsPtr, long userId, IntPtr callbackData,
                AcceptInviteCallback callback);

            internal RegisterCommandMethod RegisterCommand;

            internal RegisterSteamMethod RegisterSteam;

            internal UpdateActivityMethod UpdateActivity;

            internal ClearActivityMethod ClearActivity;

            internal SendRequestReplyMethod SendRequestReply;

            internal SendInviteMethod SendInvite;

            internal AcceptInviteMethod AcceptInvite;
        }
    }

    public partial class RelationshipManager
    {
        public delegate bool FilterHandler(ref Relationship relationship);

        public delegate void RefreshHandler();

        public delegate void RelationshipUpdateHandler(ref Relationship relationship);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal RelationshipManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event RefreshHandler OnRefresh;

        public event RelationshipUpdateHandler OnRelationshipUpdate;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnRefresh = OnRefreshImpl;
            events.OnRelationshipUpdate = OnRelationshipUpdateImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        [MonoPInvokeCallback]
        private static bool FilterCallbackImpl(IntPtr ptr, ref Relationship relationship)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (FilterHandler)h.Target;
            return callback(ref relationship);
        }

        public void Filter(FilterHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.Filter(MethodsPtr, GCHandle.ToIntPtr(wrapped), FilterCallbackImpl);
            wrapped.Free();
        }

        public int Count()
        {
            var ret = new int();
            var res = Methods.Count(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public Relationship Get(long userId)
        {
            var ret = new Relationship();
            var res = Methods.Get(MethodsPtr, userId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public Relationship GetAt(uint index)
        {
            var ret = new Relationship();
            var res = Methods.GetAt(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void OnRefreshImpl(IntPtr ptr)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.RelationshipManagerInstance.OnRefresh != null)
            {
                d.RelationshipManagerInstance.OnRefresh.Invoke();
            }
        }

        [MonoPInvokeCallback]
        private static void OnRelationshipUpdateImpl(IntPtr ptr, ref Relationship relationship)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.RelationshipManagerInstance.OnRelationshipUpdate != null)
            {
                d.RelationshipManagerInstance.OnRelationshipUpdate.Invoke(ref relationship);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void RefreshHandler(IntPtr ptr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void RelationshipUpdateHandler(IntPtr ptr, ref Relationship relationship);

            internal RefreshHandler OnRefresh;

            internal RelationshipUpdateHandler OnRelationshipUpdate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate bool FilterCallback(IntPtr ptr, ref Relationship relationship);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FilterMethod(IntPtr methodsPtr, IntPtr callbackData, FilterCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result CountMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMethod(IntPtr methodsPtr, long userId, ref Relationship relationship);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetAtMethod(IntPtr methodsPtr, uint index, ref Relationship relationship);

            internal FilterMethod Filter;

            internal CountMethod Count;

            internal GetMethod Get;

            internal GetAtMethod GetAt;
        }
    }

    public partial class LobbyManager
    {
        public delegate void ConnectLobbyHandler(Result result, ref Lobby lobby);

        public delegate void ConnectLobbyWithActivitySecretHandler(Result result, ref Lobby lobby);

        public delegate void ConnectVoiceHandler(Result result);

        public delegate void CreateLobbyHandler(Result result, ref Lobby lobby);

        public delegate void DeleteLobbyHandler(Result result);

        public delegate void DisconnectLobbyHandler(Result result);

        public delegate void DisconnectVoiceHandler(Result result);

        public delegate void LobbyDeleteHandler(long lobbyId, uint reason);

        public delegate void LobbyMessageHandler(long lobbyId, long userId, byte[] data);

        public delegate void LobbyUpdateHandler(long lobbyId);

        public delegate void MemberConnectHandler(long lobbyId, long userId);

        public delegate void MemberDisconnectHandler(long lobbyId, long userId);

        public delegate void MemberUpdateHandler(long lobbyId, long userId);

        public delegate void NetworkMessageHandler(long lobbyId, long userId, byte channelId, byte[] data);

        public delegate void SearchHandler(Result result);

        public delegate void SendLobbyMessageHandler(Result result);

        public delegate void SpeakingHandler(long lobbyId, long userId, bool speaking);

        public delegate void UpdateLobbyHandler(Result result);

        public delegate void UpdateMemberHandler(Result result);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal LobbyManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event LobbyUpdateHandler OnLobbyUpdate;

        public event LobbyDeleteHandler OnLobbyDelete;

        public event MemberConnectHandler OnMemberConnect;

        public event MemberUpdateHandler OnMemberUpdate;

        public event MemberDisconnectHandler OnMemberDisconnect;

        public event LobbyMessageHandler OnLobbyMessage;

        public event SpeakingHandler OnSpeaking;

        public event NetworkMessageHandler OnNetworkMessage;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnLobbyUpdate = OnLobbyUpdateImpl;
            events.OnLobbyDelete = OnLobbyDeleteImpl;
            events.OnMemberConnect = OnMemberConnectImpl;
            events.OnMemberUpdate = OnMemberUpdateImpl;
            events.OnMemberDisconnect = OnMemberDisconnectImpl;
            events.OnLobbyMessage = OnLobbyMessageImpl;
            events.OnSpeaking = OnSpeakingImpl;
            events.OnNetworkMessage = OnNetworkMessageImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public LobbyTransaction GetLobbyCreateTransaction()
        {
            var ret = new LobbyTransaction();
            var res = Methods.GetLobbyCreateTransaction(MethodsPtr, ref ret.MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public LobbyTransaction GetLobbyUpdateTransaction(long lobbyId)
        {
            var ret = new LobbyTransaction();
            var res = Methods.GetLobbyUpdateTransaction(MethodsPtr, lobbyId, ref ret.MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public LobbyMemberTransaction GetMemberUpdateTransaction(long lobbyId, long userId)
        {
            var ret = new LobbyMemberTransaction();
            var res = Methods.GetMemberUpdateTransaction(MethodsPtr, lobbyId, userId, ref ret.MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void CreateLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (CreateLobbyHandler)h.Target;
            h.Free();
            callback(result, ref lobby);
        }

        public void CreateLobby(LobbyTransaction transaction, CreateLobbyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.CreateLobby(MethodsPtr, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), CreateLobbyCallbackImpl);
            transaction.MethodsPtr = IntPtr.Zero;
        }

        [MonoPInvokeCallback]
        private static void UpdateLobbyCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (UpdateLobbyHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void UpdateLobby(long lobbyId, LobbyTransaction transaction, UpdateLobbyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.UpdateLobby(MethodsPtr, lobbyId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped),
                UpdateLobbyCallbackImpl);
            transaction.MethodsPtr = IntPtr.Zero;
        }

        [MonoPInvokeCallback]
        private static void DeleteLobbyCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (DeleteLobbyHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void DeleteLobby(long lobbyId, DeleteLobbyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.DeleteLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DeleteLobbyCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void ConnectLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ConnectLobbyHandler)h.Target;
            h.Free();
            callback(result, ref lobby);
        }

        public void ConnectLobby(long lobbyId, string secret, ConnectLobbyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ConnectLobby(MethodsPtr, lobbyId, secret, GCHandle.ToIntPtr(wrapped), ConnectLobbyCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void ConnectLobbyWithActivitySecretCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ConnectLobbyWithActivitySecretHandler)h.Target;
            h.Free();
            callback(result, ref lobby);
        }

        public void ConnectLobbyWithActivitySecret(string activitySecret, ConnectLobbyWithActivitySecretHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ConnectLobbyWithActivitySecret(MethodsPtr, activitySecret, GCHandle.ToIntPtr(wrapped),
                ConnectLobbyWithActivitySecretCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void DisconnectLobbyCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (DisconnectLobbyHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void DisconnectLobby(long lobbyId, DisconnectLobbyHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.DisconnectLobby(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DisconnectLobbyCallbackImpl);
        }

        public Lobby GetLobby(long lobbyId)
        {
            var ret = new Lobby();
            var res = Methods.GetLobby(MethodsPtr, lobbyId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public string GetLobbyActivitySecret(long lobbyId)
        {
            var ret = new StringBuilder(128);
            var res = Methods.GetLobbyActivitySecret(MethodsPtr, lobbyId, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        public string GetLobbyMetadataValue(long lobbyId, string key)
        {
            var ret = new StringBuilder(4096);
            var res = Methods.GetLobbyMetadataValue(MethodsPtr, lobbyId, key, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        public string GetLobbyMetadataKey(long lobbyId, int index)
        {
            var ret = new StringBuilder(256);
            var res = Methods.GetLobbyMetadataKey(MethodsPtr, lobbyId, index, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        public int LobbyMetadataCount(long lobbyId)
        {
            var ret = new int();
            var res = Methods.LobbyMetadataCount(MethodsPtr, lobbyId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public int MemberCount(long lobbyId)
        {
            var ret = new int();
            var res = Methods.MemberCount(MethodsPtr, lobbyId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public long GetMemberUserId(long lobbyId, int index)
        {
            var ret = new long();
            var res = Methods.GetMemberUserId(MethodsPtr, lobbyId, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public User GetMemberUser(long lobbyId, long userId)
        {
            var ret = new User();
            var res = Methods.GetMemberUser(MethodsPtr, lobbyId, userId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public string GetMemberMetadataValue(long lobbyId, long userId, string key)
        {
            var ret = new StringBuilder(4096);
            var res = Methods.GetMemberMetadataValue(MethodsPtr, lobbyId, userId, key, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        public string GetMemberMetadataKey(long lobbyId, long userId, int index)
        {
            var ret = new StringBuilder(256);
            var res = Methods.GetMemberMetadataKey(MethodsPtr, lobbyId, userId, index, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        public int MemberMetadataCount(long lobbyId, long userId)
        {
            var ret = new int();
            var res = Methods.MemberMetadataCount(MethodsPtr, lobbyId, userId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void UpdateMemberCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (UpdateMemberHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void UpdateMember(long lobbyId, long userId, LobbyMemberTransaction transaction, UpdateMemberHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.UpdateMember(MethodsPtr, lobbyId, userId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped),
                UpdateMemberCallbackImpl);
            transaction.MethodsPtr = IntPtr.Zero;
        }

        [MonoPInvokeCallback]
        private static void SendLobbyMessageCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SendLobbyMessageHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SendLobbyMessage(long lobbyId, byte[] data, SendLobbyMessageHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SendLobbyMessage(MethodsPtr, lobbyId, data, data.Length, GCHandle.ToIntPtr(wrapped),
                SendLobbyMessageCallbackImpl);
        }

        public LobbySearchQuery GetSearchQuery()
        {
            var ret = new LobbySearchQuery();
            var res = Methods.GetSearchQuery(MethodsPtr, ref ret.MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void SearchCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SearchHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void Search(LobbySearchQuery query, SearchHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.Search(MethodsPtr, query.MethodsPtr, GCHandle.ToIntPtr(wrapped), SearchCallbackImpl);
            query.MethodsPtr = IntPtr.Zero;
        }

        public int LobbyCount()
        {
            var ret = new int();
            Methods.LobbyCount(MethodsPtr, ref ret);
            return ret;
        }

        public long GetLobbyId(int index)
        {
            var ret = new long();
            var res = Methods.GetLobbyId(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void ConnectVoiceCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ConnectVoiceHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void ConnectVoice(long lobbyId, ConnectVoiceHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ConnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), ConnectVoiceCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void DisconnectVoiceCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (DisconnectVoiceHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void DisconnectVoice(long lobbyId, DisconnectVoiceHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.DisconnectVoice(MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DisconnectVoiceCallbackImpl);
        }

        public void ConnectNetwork(long lobbyId)
        {
            var res = Methods.ConnectNetwork(MethodsPtr, lobbyId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public void DisconnectNetwork(long lobbyId)
        {
            var res = Methods.DisconnectNetwork(MethodsPtr, lobbyId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public void FlushNetwork()
        {
            var res = Methods.FlushNetwork(MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public void OpenNetworkChannel(long lobbyId, byte channelId, bool reliable)
        {
            var res = Methods.OpenNetworkChannel(MethodsPtr, lobbyId, channelId, reliable);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public void SendNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
        {
            var res = Methods.SendNetworkMessage(MethodsPtr, lobbyId, userId, channelId, data, data.Length);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void OnLobbyUpdateImpl(IntPtr ptr, long lobbyId)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnLobbyUpdate != null)
            {
                d.LobbyManagerInstance.OnLobbyUpdate.Invoke(lobbyId);
            }
        }

        [MonoPInvokeCallback]
        private static void OnLobbyDeleteImpl(IntPtr ptr, long lobbyId, uint reason)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnLobbyDelete != null)
            {
                d.LobbyManagerInstance.OnLobbyDelete.Invoke(lobbyId, reason);
            }
        }

        [MonoPInvokeCallback]
        private static void OnMemberConnectImpl(IntPtr ptr, long lobbyId, long userId)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnMemberConnect != null)
            {
                d.LobbyManagerInstance.OnMemberConnect.Invoke(lobbyId, userId);
            }
        }

        [MonoPInvokeCallback]
        private static void OnMemberUpdateImpl(IntPtr ptr, long lobbyId, long userId)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnMemberUpdate != null)
            {
                d.LobbyManagerInstance.OnMemberUpdate.Invoke(lobbyId, userId);
            }
        }

        [MonoPInvokeCallback]
        private static void OnMemberDisconnectImpl(IntPtr ptr, long lobbyId, long userId)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnMemberDisconnect != null)
            {
                d.LobbyManagerInstance.OnMemberDisconnect.Invoke(lobbyId, userId);
            }
        }

        [MonoPInvokeCallback]
        private static void OnLobbyMessageImpl(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnLobbyMessage != null)
            {
                var data = new byte[dataLen];
                Marshal.Copy(dataPtr, data, 0, dataLen);
                d.LobbyManagerInstance.OnLobbyMessage.Invoke(lobbyId, userId, data);
            }
        }

        [MonoPInvokeCallback]
        private static void OnSpeakingImpl(IntPtr ptr, long lobbyId, long userId, bool speaking)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnSpeaking != null)
            {
                d.LobbyManagerInstance.OnSpeaking.Invoke(lobbyId, userId, speaking);
            }
        }

        [MonoPInvokeCallback]
        private static void OnNetworkMessageImpl(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr,
            int dataLen)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.LobbyManagerInstance.OnNetworkMessage != null)
            {
                var data = new byte[dataLen];
                Marshal.Copy(dataPtr, data, 0, dataLen);
                d.LobbyManagerInstance.OnNetworkMessage.Invoke(lobbyId, userId, channelId, data);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void LobbyUpdateHandler(IntPtr ptr, long lobbyId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void LobbyDeleteHandler(IntPtr ptr, long lobbyId, uint reason);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void MemberConnectHandler(IntPtr ptr, long lobbyId, long userId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void MemberUpdateHandler(IntPtr ptr, long lobbyId, long userId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void MemberDisconnectHandler(IntPtr ptr, long lobbyId, long userId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void LobbyMessageHandler(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SpeakingHandler(IntPtr ptr, long lobbyId, long userId, bool speaking);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void NetworkMessageHandler(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr,
                int dataLen);

            internal LobbyUpdateHandler OnLobbyUpdate;

            internal LobbyDeleteHandler OnLobbyDelete;

            internal MemberConnectHandler OnMemberConnect;

            internal MemberUpdateHandler OnMemberUpdate;

            internal MemberDisconnectHandler OnMemberDisconnect;

            internal LobbyMessageHandler OnLobbyMessage;

            internal SpeakingHandler OnSpeaking;

            internal NetworkMessageHandler OnNetworkMessage;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyCreateTransactionMethod(IntPtr methodsPtr, ref IntPtr transaction);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, ref IntPtr transaction);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMemberUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, long userId,
                ref IntPtr transaction);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CreateLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CreateLobbyMethod(IntPtr methodsPtr, IntPtr transaction, IntPtr callbackData,
                CreateLobbyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateLobbyCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr transaction, IntPtr callbackData,
                UpdateLobbyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DeleteLobbyCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DeleteLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData,
                DeleteLobbyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectLobbyMethod(IntPtr methodsPtr, long lobbyId,
                [MarshalAs(UnmanagedType.LPStr)] string secret, IntPtr callbackData, ConnectLobbyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectLobbyWithActivitySecretCallback(IntPtr ptr, Result result, ref Lobby lobby);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectLobbyWithActivitySecretMethod(IntPtr methodsPtr,
                [MarshalAs(UnmanagedType.LPStr)] string activitySecret, IntPtr callbackData,
                ConnectLobbyWithActivitySecretCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DisconnectLobbyCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DisconnectLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData,
                DisconnectLobbyCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyMethod(IntPtr methodsPtr, long lobbyId, ref Lobby lobby);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyActivitySecretMethod(IntPtr methodsPtr, long lobbyId, StringBuilder secret);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyMetadataValueMethod(IntPtr methodsPtr, long lobbyId,
                [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, int index, StringBuilder key);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result LobbyMetadataCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result MemberCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMemberUserIdMethod(IntPtr methodsPtr, long lobbyId, int index, ref long userId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMemberUserMethod(IntPtr methodsPtr, long lobbyId, long userId, ref User user);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMemberMetadataValueMethod(IntPtr methodsPtr, long lobbyId, long userId,
                [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetMemberMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, long userId, int index,
                StringBuilder key);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result MemberMetadataCountMethod(IntPtr methodsPtr, long lobbyId, long userId, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateMemberCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UpdateMemberMethod(IntPtr methodsPtr, long lobbyId, long userId, IntPtr transaction,
                IntPtr callbackData, UpdateMemberCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendLobbyMessageCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SendLobbyMessageMethod(IntPtr methodsPtr, long lobbyId, byte[] data, int dataLen,
                IntPtr callbackData, SendLobbyMessageCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetSearchQueryMethod(IntPtr methodsPtr, ref IntPtr query);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SearchCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SearchMethod(IntPtr methodsPtr, IntPtr query, IntPtr callbackData, SearchCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void LobbyCountMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLobbyIdMethod(IntPtr methodsPtr, int index, ref long lobbyId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectVoiceCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ConnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData,
                ConnectVoiceCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DisconnectVoiceCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void DisconnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData,
                DisconnectVoiceCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result ConnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result DisconnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result FlushNetworkMethod(IntPtr methodsPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result OpenNetworkChannelMethod(IntPtr methodsPtr, long lobbyId, byte channelId, bool reliable);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SendNetworkMessageMethod(IntPtr methodsPtr, long lobbyId, long userId, byte channelId,
                byte[] data, int dataLen);

            internal GetLobbyCreateTransactionMethod GetLobbyCreateTransaction;

            internal GetLobbyUpdateTransactionMethod GetLobbyUpdateTransaction;

            internal GetMemberUpdateTransactionMethod GetMemberUpdateTransaction;

            internal CreateLobbyMethod CreateLobby;

            internal UpdateLobbyMethod UpdateLobby;

            internal DeleteLobbyMethod DeleteLobby;

            internal ConnectLobbyMethod ConnectLobby;

            internal ConnectLobbyWithActivitySecretMethod ConnectLobbyWithActivitySecret;

            internal DisconnectLobbyMethod DisconnectLobby;

            internal GetLobbyMethod GetLobby;

            internal GetLobbyActivitySecretMethod GetLobbyActivitySecret;

            internal GetLobbyMetadataValueMethod GetLobbyMetadataValue;

            internal GetLobbyMetadataKeyMethod GetLobbyMetadataKey;

            internal LobbyMetadataCountMethod LobbyMetadataCount;

            internal MemberCountMethod MemberCount;

            internal GetMemberUserIdMethod GetMemberUserId;

            internal GetMemberUserMethod GetMemberUser;

            internal GetMemberMetadataValueMethod GetMemberMetadataValue;

            internal GetMemberMetadataKeyMethod GetMemberMetadataKey;

            internal MemberMetadataCountMethod MemberMetadataCount;

            internal UpdateMemberMethod UpdateMember;

            internal SendLobbyMessageMethod SendLobbyMessage;

            internal GetSearchQueryMethod GetSearchQuery;

            internal SearchMethod Search;

            internal LobbyCountMethod LobbyCount;

            internal GetLobbyIdMethod GetLobbyId;

            internal ConnectVoiceMethod ConnectVoice;

            internal DisconnectVoiceMethod DisconnectVoice;

            internal ConnectNetworkMethod ConnectNetwork;

            internal DisconnectNetworkMethod DisconnectNetwork;

            internal FlushNetworkMethod FlushNetwork;

            internal OpenNetworkChannelMethod OpenNetworkChannel;

            internal SendNetworkMessageMethod SendNetworkMessage;
        }
    }

    public partial class NetworkManager
    {
        public delegate void MessageHandler(ulong peerId, byte channelId, byte[] data);

        public delegate void RouteUpdateHandler(string routeData);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal NetworkManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event MessageHandler OnMessage;

        public event RouteUpdateHandler OnRouteUpdate;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnMessage = OnMessageImpl;
            events.OnRouteUpdate = OnRouteUpdateImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        /// <summary>
        ///     Get the local peer ID for this process.
        /// </summary>
        public ulong GetPeerId()
        {
            var ret = new ulong();
            Methods.GetPeerId(MethodsPtr, ref ret);
            return ret;
        }

        /// <summary>
        ///     Send pending network messages.
        /// </summary>
        public void Flush()
        {
            var res = Methods.Flush(MethodsPtr);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Open a connection to a remote peer.
        /// </summary>
        public void OpenPeer(ulong peerId, string routeData)
        {
            var res = Methods.OpenPeer(MethodsPtr, peerId, routeData);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Update the route data for a connected peer.
        /// </summary>
        public void UpdatePeer(ulong peerId, string routeData)
        {
            var res = Methods.UpdatePeer(MethodsPtr, peerId, routeData);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Close the connection to a remote peer.
        /// </summary>
        public void ClosePeer(ulong peerId)
        {
            var res = Methods.ClosePeer(MethodsPtr, peerId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Open a message channel to a connected peer.
        /// </summary>
        public void OpenChannel(ulong peerId, byte channelId, bool reliable)
        {
            var res = Methods.OpenChannel(MethodsPtr, peerId, channelId, reliable);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Close a message channel to a connected peer.
        /// </summary>
        public void CloseChannel(ulong peerId, byte channelId)
        {
            var res = Methods.CloseChannel(MethodsPtr, peerId, channelId);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        /// <summary>
        ///     Send a message to a connected peer over an opened message channel.
        /// </summary>
        public void SendMessage(ulong peerId, byte channelId, byte[] data)
        {
            var res = Methods.SendMessage(MethodsPtr, peerId, channelId, data, data.Length);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void OnMessageImpl(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.NetworkManagerInstance.OnMessage != null)
            {
                var data = new byte[dataLen];
                Marshal.Copy(dataPtr, data, 0, dataLen);
                d.NetworkManagerInstance.OnMessage.Invoke(peerId, channelId, data);
            }
        }

        [MonoPInvokeCallback]
        private static void OnRouteUpdateImpl(IntPtr ptr, string routeData)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.NetworkManagerInstance.OnRouteUpdate != null)
            {
                d.NetworkManagerInstance.OnRouteUpdate.Invoke(routeData);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void MessageHandler(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void RouteUpdateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string routeData);

            internal MessageHandler OnMessage;

            internal RouteUpdateHandler OnRouteUpdate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void GetPeerIdMethod(IntPtr methodsPtr, ref ulong peerId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result FlushMethod(IntPtr methodsPtr);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result OpenPeerMethod(IntPtr methodsPtr, ulong peerId,
                [MarshalAs(UnmanagedType.LPStr)] string routeData);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result UpdatePeerMethod(IntPtr methodsPtr, ulong peerId,
                [MarshalAs(UnmanagedType.LPStr)] string routeData);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result ClosePeerMethod(IntPtr methodsPtr, ulong peerId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result OpenChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId, bool reliable);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result CloseChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SendMessageMethod(IntPtr methodsPtr, ulong peerId, byte channelId, byte[] data,
                int dataLen);

            internal GetPeerIdMethod GetPeerId;

            internal FlushMethod Flush;

            internal OpenPeerMethod OpenPeer;

            internal UpdatePeerMethod UpdatePeer;

            internal ClosePeerMethod ClosePeer;

            internal OpenChannelMethod OpenChannel;

            internal CloseChannelMethod CloseChannel;

            internal SendMessageMethod SendMessage;
        }
    }

    public partial class OverlayManager
    {
        public delegate void OpenActivityInviteHandler(Result result);

        public delegate void OpenGuildInviteHandler(Result result);

        public delegate void OpenVoiceSettingsHandler(Result result);

        public delegate void SetLockedHandler(Result result);

        public delegate void ToggleHandler(bool locked);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal OverlayManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event ToggleHandler OnToggle;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnToggle = OnToggleImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public bool IsEnabled()
        {
            var ret = new bool();
            Methods.IsEnabled(MethodsPtr, ref ret);
            return ret;
        }

        public bool IsLocked()
        {
            var ret = new bool();
            Methods.IsLocked(MethodsPtr, ref ret);
            return ret;
        }

        [MonoPInvokeCallback]
        private static void SetLockedCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SetLockedHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SetLocked(bool locked, SetLockedHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SetLocked(MethodsPtr, locked, GCHandle.ToIntPtr(wrapped), SetLockedCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OpenActivityInviteCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (OpenActivityInviteHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void OpenActivityInvite(ActivityActionType type, OpenActivityInviteHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.OpenActivityInvite(MethodsPtr, type, GCHandle.ToIntPtr(wrapped), OpenActivityInviteCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OpenGuildInviteCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (OpenGuildInviteHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void OpenGuildInvite(string code, OpenGuildInviteHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.OpenGuildInvite(MethodsPtr, code, GCHandle.ToIntPtr(wrapped), OpenGuildInviteCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OpenVoiceSettingsCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (OpenVoiceSettingsHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void OpenVoiceSettings(OpenVoiceSettingsHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.OpenVoiceSettings(MethodsPtr, GCHandle.ToIntPtr(wrapped), OpenVoiceSettingsCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OnToggleImpl(IntPtr ptr, bool locked)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.OverlayManagerInstance.OnToggle != null)
            {
                d.OverlayManagerInstance.OnToggle.Invoke(locked);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ToggleHandler(IntPtr ptr, bool locked);

            internal ToggleHandler OnToggle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void IsEnabledMethod(IntPtr methodsPtr, ref bool enabled);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void IsLockedMethod(IntPtr methodsPtr, ref bool locked);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetLockedCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetLockedMethod(IntPtr methodsPtr, bool locked, IntPtr callbackData,
                SetLockedCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenActivityInviteCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenActivityInviteMethod(IntPtr methodsPtr, ActivityActionType type, IntPtr callbackData,
                OpenActivityInviteCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenGuildInviteCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenGuildInviteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string code,
                IntPtr callbackData, OpenGuildInviteCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenVoiceSettingsCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void OpenVoiceSettingsMethod(IntPtr methodsPtr, IntPtr callbackData,
                OpenVoiceSettingsCallback callback);

            internal IsEnabledMethod IsEnabled;

            internal IsLockedMethod IsLocked;

            internal SetLockedMethod SetLocked;

            internal OpenActivityInviteMethod OpenActivityInvite;

            internal OpenGuildInviteMethod OpenGuildInvite;

            internal OpenVoiceSettingsMethod OpenVoiceSettings;
        }
    }

    public partial class StorageManager
    {
        public delegate void ReadAsyncHandler(Result result, byte[] data);

        public delegate void ReadAsyncPartialHandler(Result result, byte[] data);

        public delegate void WriteAsyncHandler(Result result);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal StorageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public uint Read(string name, byte[] data)
        {
            var ret = new uint();
            var res = Methods.Read(MethodsPtr, name, data, data.Length, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void ReadAsyncCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ReadAsyncHandler)h.Target;
            h.Free();
            var data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            callback(result, data);
        }

        public void ReadAsync(string name, ReadAsyncHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ReadAsync(MethodsPtr, name, GCHandle.ToIntPtr(wrapped), ReadAsyncCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void ReadAsyncPartialCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (ReadAsyncPartialHandler)h.Target;
            h.Free();
            var data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            callback(result, data);
        }

        public void ReadAsyncPartial(string name, ulong offset, ulong length, ReadAsyncPartialHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.ReadAsyncPartial(MethodsPtr, name, offset, length, GCHandle.ToIntPtr(wrapped),
                ReadAsyncPartialCallbackImpl);
        }

        public void Write(string name, byte[] data)
        {
            var res = Methods.Write(MethodsPtr, name, data, data.Length);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void WriteAsyncCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (WriteAsyncHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void WriteAsync(string name, byte[] data, WriteAsyncHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.WriteAsync(MethodsPtr, name, data, data.Length, GCHandle.ToIntPtr(wrapped), WriteAsyncCallbackImpl);
        }

        public void Delete(string name)
        {
            var res = Methods.Delete(MethodsPtr, name);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public bool Exists(string name)
        {
            var ret = new bool();
            var res = Methods.Exists(MethodsPtr, name, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public int Count()
        {
            var ret = new int();
            Methods.Count(MethodsPtr, ref ret);
            return ret;
        }

        public FileStat Stat(string name)
        {
            var ret = new FileStat();
            var res = Methods.Stat(MethodsPtr, name, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public FileStat StatAt(int index)
        {
            var ret = new FileStat();
            var res = Methods.StatAt(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public string GetPath()
        {
            var ret = new StringBuilder(4096);
            var res = Methods.GetPath(MethodsPtr, ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result ReadMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data,
                int dataLen, ref uint read);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ReadAsyncCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ReadAsyncMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name,
                IntPtr callbackData, ReadAsyncCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ReadAsyncPartialCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void ReadAsyncPartialMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name,
                ulong offset, ulong length, IntPtr callbackData, ReadAsyncPartialCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result WriteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data,
                int dataLen);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void WriteAsyncCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void WriteAsyncMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name,
                byte[] data, int dataLen, IntPtr callbackData, WriteAsyncCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result DeleteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result ExistsMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name,
                ref bool exists);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CountMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result StatMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name,
                ref FileStat stat);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result StatAtMethod(IntPtr methodsPtr, int index, ref FileStat stat);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetPathMethod(IntPtr methodsPtr, StringBuilder path);

            internal ReadMethod Read;

            internal ReadAsyncMethod ReadAsync;

            internal ReadAsyncPartialMethod ReadAsyncPartial;

            internal WriteMethod Write;

            internal WriteAsyncMethod WriteAsync;

            internal DeleteMethod Delete;

            internal ExistsMethod Exists;

            internal CountMethod Count;

            internal StatMethod Stat;

            internal StatAtMethod StatAt;

            internal GetPathMethod GetPath;
        }
    }

    public partial class StoreManager
    {
        public delegate void EntitlementCreateHandler(ref Entitlement entitlement);

        public delegate void EntitlementDeleteHandler(ref Entitlement entitlement);

        public delegate void FetchEntitlementsHandler(Result result);

        public delegate void FetchSkusHandler(Result result);

        public delegate void StartPurchaseHandler(Result result);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal StoreManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event EntitlementCreateHandler OnEntitlementCreate;

        public event EntitlementDeleteHandler OnEntitlementDelete;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnEntitlementCreate = OnEntitlementCreateImpl;
            events.OnEntitlementDelete = OnEntitlementDeleteImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        [MonoPInvokeCallback]
        private static void FetchSkusCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (FetchSkusHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void FetchSkus(FetchSkusHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.FetchSkus(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchSkusCallbackImpl);
        }

        public int CountSkus()
        {
            var ret = new int();
            Methods.CountSkus(MethodsPtr, ref ret);
            return ret;
        }

        public Sku GetSku(long skuId)
        {
            var ret = new Sku();
            var res = Methods.GetSku(MethodsPtr, skuId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public Sku GetSkuAt(int index)
        {
            var ret = new Sku();
            var res = Methods.GetSkuAt(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void FetchEntitlementsCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (FetchEntitlementsHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void FetchEntitlements(FetchEntitlementsHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.FetchEntitlements(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchEntitlementsCallbackImpl);
        }

        public int CountEntitlements()
        {
            var ret = new int();
            Methods.CountEntitlements(MethodsPtr, ref ret);
            return ret;
        }

        public Entitlement GetEntitlement(long entitlementId)
        {
            var ret = new Entitlement();
            var res = Methods.GetEntitlement(MethodsPtr, entitlementId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public Entitlement GetEntitlementAt(int index)
        {
            var ret = new Entitlement();
            var res = Methods.GetEntitlementAt(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public bool HasSkuEntitlement(long skuId)
        {
            var ret = new bool();
            var res = Methods.HasSkuEntitlement(MethodsPtr, skuId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void StartPurchaseCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (StartPurchaseHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void StartPurchase(long skuId, StartPurchaseHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.StartPurchase(MethodsPtr, skuId, GCHandle.ToIntPtr(wrapped), StartPurchaseCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void OnEntitlementCreateImpl(IntPtr ptr, ref Entitlement entitlement)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.StoreManagerInstance.OnEntitlementCreate != null)
            {
                d.StoreManagerInstance.OnEntitlementCreate.Invoke(ref entitlement);
            }
        }

        [MonoPInvokeCallback]
        private static void OnEntitlementDeleteImpl(IntPtr ptr, ref Entitlement entitlement)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.StoreManagerInstance.OnEntitlementDelete != null)
            {
                d.StoreManagerInstance.OnEntitlementDelete.Invoke(ref entitlement);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void EntitlementCreateHandler(IntPtr ptr, ref Entitlement entitlement);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void EntitlementDeleteHandler(IntPtr ptr, ref Entitlement entitlement);

            internal EntitlementCreateHandler OnEntitlementCreate;

            internal EntitlementDeleteHandler OnEntitlementDelete;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchSkusCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchSkusMethod(IntPtr methodsPtr, IntPtr callbackData, FetchSkusCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CountSkusMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetSkuMethod(IntPtr methodsPtr, long skuId, ref Sku sku);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetSkuAtMethod(IntPtr methodsPtr, int index, ref Sku sku);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchEntitlementsCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchEntitlementsMethod(IntPtr methodsPtr, IntPtr callbackData,
                FetchEntitlementsCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CountEntitlementsMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetEntitlementMethod(IntPtr methodsPtr, long entitlementId, ref Entitlement entitlement);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetEntitlementAtMethod(IntPtr methodsPtr, int index, ref Entitlement entitlement);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result HasSkuEntitlementMethod(IntPtr methodsPtr, long skuId, ref bool hasEntitlement);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void StartPurchaseCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void StartPurchaseMethod(IntPtr methodsPtr, long skuId, IntPtr callbackData,
                StartPurchaseCallback callback);

            internal FetchSkusMethod FetchSkus;

            internal CountSkusMethod CountSkus;

            internal GetSkuMethod GetSku;

            internal GetSkuAtMethod GetSkuAt;

            internal FetchEntitlementsMethod FetchEntitlements;

            internal CountEntitlementsMethod CountEntitlements;

            internal GetEntitlementMethod GetEntitlement;

            internal GetEntitlementAtMethod GetEntitlementAt;

            internal HasSkuEntitlementMethod HasSkuEntitlement;

            internal StartPurchaseMethod StartPurchase;
        }
    }

    public partial class VoiceManager
    {
        public delegate void SetInputModeHandler(Result result);

        public delegate void SettingsUpdateHandler();

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal VoiceManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event SettingsUpdateHandler OnSettingsUpdate;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnSettingsUpdate = OnSettingsUpdateImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        public InputMode GetInputMode()
        {
            var ret = new InputMode();
            var res = Methods.GetInputMode(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void SetInputModeCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SetInputModeHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SetInputMode(InputMode inputMode, SetInputModeHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SetInputMode(MethodsPtr, inputMode, GCHandle.ToIntPtr(wrapped), SetInputModeCallbackImpl);
        }

        public bool IsSelfMute()
        {
            var ret = new bool();
            var res = Methods.IsSelfMute(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public void SetSelfMute(bool mute)
        {
            var res = Methods.SetSelfMute(MethodsPtr, mute);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public bool IsSelfDeaf()
        {
            var ret = new bool();
            var res = Methods.IsSelfDeaf(MethodsPtr, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public void SetSelfDeaf(bool deaf)
        {
            var res = Methods.SetSelfDeaf(MethodsPtr, deaf);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public bool IsLocalMute(long userId)
        {
            var ret = new bool();
            var res = Methods.IsLocalMute(MethodsPtr, userId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public void SetLocalMute(long userId, bool mute)
        {
            var res = Methods.SetLocalMute(MethodsPtr, userId, mute);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        public byte GetLocalVolume(long userId)
        {
            var ret = new byte();
            var res = Methods.GetLocalVolume(MethodsPtr, userId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public void SetLocalVolume(long userId, byte volume)
        {
            var res = Methods.SetLocalVolume(MethodsPtr, userId, volume);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
        }

        [MonoPInvokeCallback]
        private static void OnSettingsUpdateImpl(IntPtr ptr)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.VoiceManagerInstance.OnSettingsUpdate != null)
            {
                d.VoiceManagerInstance.OnSettingsUpdate.Invoke();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SettingsUpdateHandler(IntPtr ptr);

            internal SettingsUpdateHandler OnSettingsUpdate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetInputModeMethod(IntPtr methodsPtr, ref InputMode inputMode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetInputModeCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetInputModeMethod(IntPtr methodsPtr, InputMode inputMode, IntPtr callbackData,
                SetInputModeCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result IsSelfMuteMethod(IntPtr methodsPtr, ref bool mute);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetSelfMuteMethod(IntPtr methodsPtr, bool mute);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result IsSelfDeafMethod(IntPtr methodsPtr, ref bool deaf);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetSelfDeafMethod(IntPtr methodsPtr, bool deaf);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result IsLocalMuteMethod(IntPtr methodsPtr, long userId, ref bool mute);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetLocalMuteMethod(IntPtr methodsPtr, long userId, bool mute);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetLocalVolumeMethod(IntPtr methodsPtr, long userId, ref byte volume);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result SetLocalVolumeMethod(IntPtr methodsPtr, long userId, byte volume);

            internal GetInputModeMethod GetInputMode;

            internal SetInputModeMethod SetInputMode;

            internal IsSelfMuteMethod IsSelfMute;

            internal SetSelfMuteMethod SetSelfMute;

            internal IsSelfDeafMethod IsSelfDeaf;

            internal SetSelfDeafMethod SetSelfDeaf;

            internal IsLocalMuteMethod IsLocalMute;

            internal SetLocalMuteMethod SetLocalMute;

            internal GetLocalVolumeMethod GetLocalVolume;

            internal SetLocalVolumeMethod SetLocalVolume;
        }
    }

    public partial class AchievementManager
    {
        public delegate void FetchUserAchievementsHandler(Result result);

        public delegate void SetUserAchievementHandler(Result result);

        public delegate void UserAchievementUpdateHandler(ref UserAchievement userAchievement);

        private readonly IntPtr MethodsPtr;

        private object MethodsStructure;

        internal AchievementManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
        {
            if (eventsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
            InitEvents(eventsPtr, ref events);
            MethodsPtr = ptr;
            if (MethodsPtr == IntPtr.Zero)
            {
                throw new ResultException(Result.InternalError);
            }
        }

        private FFIMethods Methods
        {
            get
            {
                if (MethodsStructure == null)
                {
                    MethodsStructure = Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));
                }
                return (FFIMethods)MethodsStructure;
            }
        }

        public event UserAchievementUpdateHandler OnUserAchievementUpdate;

        private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
        {
            events.OnUserAchievementUpdate = OnUserAchievementUpdateImpl;
            Marshal.StructureToPtr(events, eventsPtr, false);
        }

        [MonoPInvokeCallback]
        private static void SetUserAchievementCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (SetUserAchievementHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void SetUserAchievement(long achievementId, byte percentComplete, SetUserAchievementHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.SetUserAchievement(MethodsPtr, achievementId, percentComplete, GCHandle.ToIntPtr(wrapped),
                SetUserAchievementCallbackImpl);
        }

        [MonoPInvokeCallback]
        private static void FetchUserAchievementsCallbackImpl(IntPtr ptr, Result result)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var callback = (FetchUserAchievementsHandler)h.Target;
            h.Free();
            callback(result);
        }

        public void FetchUserAchievements(FetchUserAchievementsHandler callback)
        {
            var wrapped = GCHandle.Alloc(callback);
            Methods.FetchUserAchievements(MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchUserAchievementsCallbackImpl);
        }

        public int CountUserAchievements()
        {
            var ret = new int();
            Methods.CountUserAchievements(MethodsPtr, ref ret);
            return ret;
        }

        public UserAchievement GetUserAchievement(long userAchievementId)
        {
            var ret = new UserAchievement();
            var res = Methods.GetUserAchievement(MethodsPtr, userAchievementId, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        public UserAchievement GetUserAchievementAt(int index)
        {
            var ret = new UserAchievement();
            var res = Methods.GetUserAchievementAt(MethodsPtr, index, ref ret);
            if (res != Result.Ok)
            {
                throw new ResultException(res);
            }
            return ret;
        }

        [MonoPInvokeCallback]
        private static void OnUserAchievementUpdateImpl(IntPtr ptr, ref UserAchievement userAchievement)
        {
            var h = GCHandle.FromIntPtr(ptr);
            var d = (Discord)h.Target;
            if (d.AchievementManagerInstance.OnUserAchievementUpdate != null)
            {
                d.AchievementManagerInstance.OnUserAchievementUpdate.Invoke(ref userAchievement);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIEvents
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void UserAchievementUpdateHandler(IntPtr ptr, ref UserAchievement userAchievement);

            internal UserAchievementUpdateHandler OnUserAchievementUpdate;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FFIMethods
        {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetUserAchievementCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void SetUserAchievementMethod(IntPtr methodsPtr, long achievementId, byte percentComplete,
                IntPtr callbackData, SetUserAchievementCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchUserAchievementsCallback(IntPtr ptr, Result result);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void FetchUserAchievementsMethod(IntPtr methodsPtr, IntPtr callbackData,
                FetchUserAchievementsCallback callback);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate void CountUserAchievementsMethod(IntPtr methodsPtr, ref int count);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetUserAchievementMethod(IntPtr methodsPtr, long userAchievementId,
                ref UserAchievement userAchievement);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            internal delegate Result GetUserAchievementAtMethod(IntPtr methodsPtr, int index,
                ref UserAchievement userAchievement);

            internal SetUserAchievementMethod SetUserAchievement;

            internal FetchUserAchievementsMethod FetchUserAchievements;

            internal CountUserAchievementsMethod CountUserAchievements;

            internal GetUserAchievementMethod GetUserAchievement;

            internal GetUserAchievementAtMethod GetUserAchievementAt;
        }
    }
}
